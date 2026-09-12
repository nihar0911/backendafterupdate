using Microsoft.EntityFrameworkCore;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojPersistence.Data;

public class VendorManagementDbContext : DbContext
{
    public VendorManagementDbContext(
        DbContextOptions<VendorManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations { get; set; }

    public DbSet<Outlet> Outlets { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<Vendor> Vendors { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<TaxRate> TaxRates { get; set; }

    public DbSet<VendorProduct> VendorProducts { get; set; }

    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }

    public DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; }

    public DbSet<Quotation> Quotations { get; set; }

    public DbSet<QuotationItem> QuotationItems { get; set; }

    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    public DbSet<Contract> Contracts { get; set; }

    public DbSet<ContractVendorAllocation> ContractVendorAllocations { get; set; }

    public DbSet<DeliveryRecord> DeliveryRecords { get; set; }

    public DbSet<Invoice> Invoices { get; set; }

    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<VendorFeedback> VendorFeedbacks { get; set; }

    public DbSet<VendorOpportunityResponse> VendorOpportunityResponses { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<SpoilageAdviceSettings> SpoilageAdviceSettings { get; set; }

    public DbSet<VendorRecommendationSettings> VendorRecommendationSettings { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organizations");

            entity.HasKey(e => e.OrganizationID);

            entity.Property(e => e.OrganizationName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Address)
                .HasMaxLength(255);

            entity.Property(e => e.Phone)
                .HasMaxLength(20);

            entity.Property(e => e.Email)
                .HasMaxLength(150);

            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        modelBuilder.Entity<Outlet>(entity =>
        {
            entity.ToTable("Outlets");

            entity.HasKey(e => e.OutletID);

            entity.Property(e => e.OutletName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Address)
                .HasMaxLength(255);

            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9,6)");

            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9,6)");

            entity.Property(e => e.PurchaseOrderApproverRole)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("Organization Manager");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(e => e.RoleID);

            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.RoleName)
                .IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(e => e.UserID);

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Outlet)
                .WithMany()
                .HasForeignKey(e => e.OutletID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendors");

            entity.HasKey(e => e.VendorID);

            entity.Property(e => e.VendorName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(150);

            entity.Property(e => e.Phone)
                .HasMaxLength(20);

            entity.Property(e => e.Address)
                .HasMaxLength(255);

            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9,6)");

            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9,6)");

            entity.Property(e => e.GSTIN)
                .HasMaxLength(20);

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.HasIndex(e => e.GSTIN)
                .IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(e => e.ProductID);

            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Category)
                .HasMaxLength(100);

            entity.Property(e => e.Unit)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne<TaxRate>()
                .WithMany()
                .HasForeignKey(e => e.TaxRateID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaxRate>(entity =>
        {
            entity.ToTable("Tax_Rates");

            entity.HasKey(e => e.TaxRateID);

            entity.Property(e => e.TaxName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Percentage)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();
        });

        modelBuilder.Entity<VendorProduct>(entity =>
        {
            entity.ToTable("Vendor_Products");

            entity.HasKey(e => e.VendorProductID);

            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(e => new
            {
                e.VendorID,
                e.ProductID
            })
            .IsUnique();

            entity.HasOne<Vendor>()
                .WithMany()
                .HasForeignKey(e => e.VendorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseRequest>(entity =>
        {
            entity.ToTable("Purchase_Requests");

            entity.HasKey(e => e.RequestID);

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne<Outlet>()
                .WithMany()
                .HasForeignKey(e => e.OutletID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseRequestItem>(entity =>
        {
            entity.ToTable("Purchase_Request_Items");

            entity.HasKey(e => e.RequestItemID);

            entity.Property(e => e.Quantity)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.Unit)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasOne(e => e.Request)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.RequestID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Quotation>(entity =>
        {
            entity.ToTable("Quotations");

            entity.HasKey(e => e.QuotationID);

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(e => e.Request)
                .WithMany()
                .HasForeignKey(e => e.RequestID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Vendor>()
                .WithMany()
                .HasForeignKey(e => e.VendorID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuotationItem>(entity =>
        {
            entity.ToTable("Quotation_Items");

            entity.HasKey(e => e.QuotationItemID);

            entity.Property(e => e.Quantity)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.TaxRate)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.TaxAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.HasOne(e => e.Quotation)
                .WithMany(q => q.QuotationItems)
                .HasForeignKey(e => e.QuotationID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.ToTable("Purchase_Orders");

            entity.HasKey(e => e.PurchaseOrderID);

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(e => e.Request)
                .WithMany()
                .HasForeignKey(e => e.RequestID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Quotation)
                .WithMany()
                .HasForeignKey(e => e.QuotationID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Outlet)
                .WithMany()
                .HasForeignKey(e => e.OutletID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Items)
                .WithOne(e => e.PurchaseOrder)
                .HasForeignKey(e => e.PurchaseOrderID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.DeliveryStatus)
                .HasMaxLength(20);

            entity.Property(e => e.ApproverRole)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.ToTable("Purchase_Order_Items");

            entity.HasKey(e => e.POItemID);

            entity.Property(e => e.Quantity)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.TaxRate)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.TaxAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.PurchaseOrderID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("Contracts");

            entity.HasKey(e => e.ContractID);

            entity.Property(e => e.TotalQuantity)
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(e => e.UsedQuantity)
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(e => e.Outlet)
                .WithMany()
                .HasForeignKey(e => e.OutletID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContractVendorAllocation>(entity =>
        {
            entity.ToTable("Contract_Vendor_Allocations");

            entity.HasKey(e => e.ContractVendorAllocationID);

            entity.Property(e => e.AllocationPercentage)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.AllocatedQuantity)
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(e => e.UsedQuantity)
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(e => e.Contract)
                .WithMany(e => e.VendorAllocations)
                .HasForeignKey(e => e.ContractID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new
            {
                e.ContractID,
                e.VendorID
            })
            .IsUnique();
        });

        modelBuilder.Entity<DeliveryRecord>(entity =>
        {
            entity.ToTable("Delivery_Records");

            entity.HasKey(e => e.DeliveryRecordID);

            entity.Property(e => e.DeliveryDate)
                .IsRequired();

            entity.Property(e => e.OrderedQuantity)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.ReceivedQuantity)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.SpoiledQuantity)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.SpoilagePercentage)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany()
                .HasForeignKey(e => e.PurchaseOrderID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PurchaseOrderItem)
                .WithMany()
                .HasForeignKey(e => e.POItemID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ConfirmedByUser)
                .WithMany()
                .HasForeignKey(e => e.ConfirmedByUserID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Subtotal)
                .HasPrecision(18, 2);

            entity.Property(e => e.TaxAmount)
                .HasPrecision(18, 2);

            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(e => e.InvoiceDocumentBase64)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.InvoiceFileName)
                .HasMaxLength(255);

            entity.Property(e => e.InvoiceContentType)
                .HasMaxLength(100);

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany()
                .HasForeignKey(e => e.PurchaseOrderID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Outlet)
                .WithMany()
                .HasForeignKey(e => e.OutletID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Items)
                .WithOne(e => e.Invoice)
                .HasForeignKey(e => e.InvoiceID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.InvoiceItemID);

            entity.Property(e => e.Quantity)
                .HasPrecision(18, 2);

            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.TaxRate)
                .HasPrecision(18, 2);

            entity.Property(e => e.Subtotal)
                .HasPrecision(18, 2);

            entity.Property(e => e.TaxAmount)
                .HasPrecision(18, 2);

            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2);

            entity.HasOne(e => e.Invoice)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.InvoiceID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");

            entity.HasKey(e => e.PaymentID);

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.TransactionReference)
                .HasMaxLength(100);

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.PaymentDate)
                .IsRequired();

            entity.HasOne(e => e.Invoice)
                .WithMany()
                .HasForeignKey(e => e.InvoiceID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendorFeedback>(entity =>
        {
            entity.ToTable("Vendor_Feedback");

            entity.HasKey(e => e.FeedbackID);

            entity.Property(e => e.Rating)
                .HasColumnType("decimal(3,2)")
                .IsRequired();

            entity.Property(e => e.ProductQualityRating)
                .HasColumnType("decimal(3,2)")
                .IsRequired();

            entity.Property(e => e.DeliveryRating)
                .HasColumnType("decimal(3,2)")
                .IsRequired();

            entity.Property(e => e.Review)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(e => e.FeedbackDate)
                .IsRequired();

            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Outlet)
                .WithMany()
                .HasForeignKey(e => e.OutletID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany()
                .HasForeignKey(e => e.PurchaseOrderID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.POItem)
                .WithMany()
                .HasForeignKey(e => e.POItemID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RatedByUser)
                .WithMany()
                .HasForeignKey(e => e.RatedByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new
            {
                e.PurchaseOrderID,
                e.POItemID
            })
            .IsUnique();
        });

        modelBuilder.Entity<VendorOpportunityResponse>(entity =>
        {
            entity.ToTable("VendorOpportunityResponses");

            entity.HasKey(e => e.ResponseID);

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500);

            entity.HasOne(e => e.PurchaseRequest)
                .WithMany()
                .HasForeignKey(e => e.RequestID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RequestItem)
                .WithMany()
                .HasForeignKey(e => e.RequestItemID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.RequestItemID, e.VendorID })
                .IsUnique()
                .HasFilter("[RequestItemID] IS NOT NULL");

            entity.HasIndex(e => new { e.RequestID, e.VendorID, e.ProductID });
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");

            entity.HasKey(e => e.NotificationID);

            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SpoilageAdviceSettings>(entity =>
        {
            entity.ToTable("SpoilageAdviceSettings");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.RecentDeliveriesCount)
                .IsRequired();

            entity.Property(e => e.TrendTolerancePercentage)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.HighWeightedSpoilageThreshold)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.HighRecentSpoilageThreshold)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.HighMaximumSpoilageThreshold)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.MediumWeightedSpoilageThreshold)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.LowWeightedSpoilageThreshold)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<VendorRecommendationSettings>(entity =>
        {
            entity.ToTable("VendorRecommendationSettings");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.QualityWeight)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.DeliveryWeight)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.PriceWeight)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.ReliabilityWeight)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.ReliabilityPointsPerReview)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.NeutralScoreForNewVendors)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.BestQualityThreshold)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.FastestDeliveryThreshold)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.HighQualityRationaleThreshold)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.PrioritizeActiveContracts)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();
        });
    }
}
