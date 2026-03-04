using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Lexa.Data;
using Lexa.Entities;

namespace Lexa.Endpoints;

public class AssignRoleRequest
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}

public class AssignRoleResponse
{
    public bool Success { get; set; }
}

[Authorize(Policy = "Permission:role:assign")]
public class AssignRoleToUserEndpoint : Endpoint<AssignRoleRequest, AssignRoleResponse>
{
    private readonly ApplicationDbContext _db;

    public AssignRoleToUserEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/roles/assign");
    }

    public override async Task HandleAsync(AssignRoleRequest req, CancellationToken ct)
    {
        var exists = await _db.UserRoles.FindAsync(new object[] { req.UserId, req.RoleId }, ct);
        if (exists != null)
        {
            await SendAsync(new AssignRoleResponse { Success = true });
            return;
        }

        var ur = new UserRole { UserId = req.UserId, RoleId = req.RoleId };
        _db.UserRoles.Add(ur);
        await _db.SaveChangesAsync(ct);
        await SendAsync(new AssignRoleResponse { Success = true });
    }
}
