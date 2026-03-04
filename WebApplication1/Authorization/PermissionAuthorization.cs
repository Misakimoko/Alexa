using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using Lexa.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Lexa.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}
[RegisterScoped]
public class PermissionHandler(ApplicationDbContext _db) : AuthorizationHandler<PermissionRequirement>
{


    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // Prefer permission claims from token to avoid DB hits
        var hasPermissionClaim = context.User?.FindAll("permission")?.Any(c => c.Value == requirement.Permission) ?? false;
        if (hasPermissionClaim)
        {
            context.Succeed(requirement);
            return;
        }

        // Fallback: check DB via user's id (sub claim)
        var sub = context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !int.TryParse(sub, out var userId))
        {
            return;
        }

        var has = await _db.UserRoles
            .AsNoTracking()
            .AnyAsync(ur => ur.UserId == userId && ur.Role.RolePermissions.Any(rp => rp.Permission.Name == requirement.Permission));

        if (has)
            context.Succeed(requirement);
    }
}
[RegisterScoped]
// Dynamic policy provider that creates policies for names starting with "Permission:"
public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options) : base(options)
    {
        _options = options.Value;
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("Permission:"))
        {
            var perm = policyName.Substring("Permission:".Length);
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(perm));
            return await Task.FromResult(policy.Build());
        }

        return await base.GetPolicyAsync(policyName);
    }
}
