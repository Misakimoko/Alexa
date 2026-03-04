using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Lexa.Data;
using Lexa.Entities;

namespace Lexa.Endpoints;

public class AssignPermissionToRoleRequest
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
}

public class AssignPermissionToRoleResponse
{
    public bool Success { get; set; }
}

[Authorize(Policy = "Permission:role:permission:assign")]
public class AssignPermissionToRoleEndpoint : Endpoint<AssignPermissionToRoleRequest, AssignPermissionToRoleResponse>
{
    private readonly ApplicationDbContext _db;

    public AssignPermissionToRoleEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/roles/permissions/assign");
    }

    public override async Task HandleAsync(AssignPermissionToRoleRequest req, CancellationToken ct)
    {
        var exists = await _db.RolePermissions.FindAsync(new object[] { req.RoleId, req.PermissionId }, ct);
        if (exists != null)
        {
            await SendAsync(new AssignPermissionToRoleResponse { Success = true });
            return;
        }

        var rp = new RolePermission { RoleId = req.RoleId, PermissionId = req.PermissionId };
        _db.RolePermissions.Add(rp);
        await _db.SaveChangesAsync(ct);
        await SendAsync(new AssignPermissionToRoleResponse { Success = true });
    }
}
