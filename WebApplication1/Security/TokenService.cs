using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Data;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;

namespace WebApplication1.Security;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;

    public TokenService(IConfiguration config, ApplicationDbContext db)
    {
        _config = config;
        _db = db;
    }

    public async Task<(string Token, DateTime ExpiresAt)> CreateTokenAsync(User user, CancellationToken ct = default)
    {
        var jwtKey = _config["Jwt:Key"] ?? "please-change-this-secret";
        var issuer = _config["Jwt:Issuer"] ?? "myapp";
        var audience = _config["Jwt:Audience"] ?? "myapp";
        var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
        };

        var roles = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(ct);

        foreach (var r in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, r));
        }

        // include permissions (from user's roles)
        var permissions = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .Distinct()
            .ToListAsync(ct);

        foreach (var p in permissions)
        {
            // use claim type "permission" for permissions
            claims.Add(new Claim("permission", p));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenStr, expires);
    }
}
