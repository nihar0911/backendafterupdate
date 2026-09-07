using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using VendorManagementprojApi.Services;
using VendorManagementprojApplication.Contracts.Infrastructure;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.Services;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;
using VendorManagementprojPersistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<IGeminiAiService, GeminiAiService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("https://localhost:7217")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vendor Management API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddDbContext<VendorManagementDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IOutletRepository, OutletRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorProductRepository, VendorProductRepository>();
builder.Services.AddScoped<ITaxRateRepository, TaxRateRepository>();
builder.Services.AddScoped<IPurchaseRequestRepository, PurchaseRequestRepository>();
builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IDeliveryRecordRepository, DeliveryRecordRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceDocumentService, InvoiceDocumentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IVendorFeedbackRepository, VendorFeedbackRepository>();
builder.Services.AddScoped<IVendorOpportunityResponseRepository, VendorOpportunityResponseRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(
            VendorManagementprojApplication
                .Features.Products.Queries.GetAllProducts.GetAllProductsQuery
        ).Assembly));

builder.Services.AddScoped<IVendorAnalysisService, VendorAnalysisService>();
builder.Services.AddScoped<IVendorPerformanceService, VendorPerformanceService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Vendor Management API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<VendorManagementDbContext>();

    var passwordHasher =
        scope.ServiceProvider
            .GetRequiredService<IPasswordHasher>();

    await context.Database.MigrateAsync();

    var roles = new[]
    {
        "Admin",
        "Organization Manager",
        "Outlet Manager",
        "Vendor Manager",
        "Purchase Manager"
    };

    foreach (var roleName in roles)
    {
        var roleExists =
            await context.Roles
                .AnyAsync(r => r.RoleName == roleName);

        if (!roleExists)
        {
            context.Roles.Add(new Role
            {
                RoleName = roleName
            });
        }
    }

    await context.SaveChangesAsync();

    var adminRole =
        await context.Roles
            .FirstAsync(r => r.RoleName == "Admin");

    var adminExists =
        await context.Users
            .AnyAsync(u =>
                u.Email == "admin@vendor.com");

    if (!adminExists)
    {
        var admin = new User
        {
            Name = "System Admin",
            Email = "admin@vendor.com",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            RoleID = adminRole.RoleID,
            OrganizationID = null,
            OutletID = null
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }

    var vendorManagerRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Vendor Manager");
    if (vendorManagerRole != null)
    {
        var abcVendorUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "abcvendor@fresh.com");
        if (abcVendorUser == null)
        {
            context.Users.Add(new User
            {
                Name = "ABC Vendor Manager",
                Email = "abcvendor@fresh.com",
                PasswordHash = passwordHasher.Hash("Vendor123"),
                RoleID = vendorManagerRole.RoleID,
                VendorID = 11
            });
            await context.SaveChangesAsync();
        }
        else if (abcVendorUser.VendorID == null)
        {
            abcVendorUser.VendorID = 11;
            await context.SaveChangesAsync();
        }
    }
}

app.Run();

