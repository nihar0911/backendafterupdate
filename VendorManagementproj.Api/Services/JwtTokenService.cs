using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApi.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is missing.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is missing.");

        var expiresMinutes =
            int.TryParse(
                _configuration["Jwt:ExpiresMinutes"],
                out var minutes)
                ? minutes
                : 120;

        var roleName = user.Role?.RoleName ?? string.Empty;

        var claims = new List<Claim>
        {
            new Claim(
                "UserID",
                user.UserID.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Name),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                roleName)
        };

        if (user.OrganizationID.HasValue)
        {
            claims.Add(
                new Claim(
                    "OrganizationID",
                    user.OrganizationID.Value.ToString()));
        }

        if (user.OutletID.HasValue)
        {
            claims.Add(
                new Claim(
                    "OutletID",
                    user.OutletID.Value.ToString()));
        }

        if (user.VendorID.HasValue)
        {
            claims.Add(
                new Claim(
                    "VendorID",
                    user.VendorID.Value.ToString()));
        }

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}