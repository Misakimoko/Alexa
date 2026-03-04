using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Lexa.Data;
using Lexa.Entities;

namespace Lexa.Endpoints;

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CreateRoleResponse
{
    public int Id { get; set; }
}

[Authorize(Policy = "Permission:role:manage")]
public class CreateRoleEndpoint : Endpoint<CreateRoleRequest, CreateRoleResponse>
{
    private readonly ApplicationDbContext _db;

    public CreateRoleEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/roles");
    }

    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
    {
        var role = new Role { Name = req.Name };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);
        await SendAsync(new CreateRoleResponse { Id = role.Id });
    }
}
