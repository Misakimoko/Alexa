using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Lexa.Data;
using Lexa.Entities;

namespace Lexa.Endpoints;

public class CreatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CreatePermissionResponse
{
    public int Id { get; set; }
}

[Authorize(Policy = "Permission:permission:manage")]
public class CreatePermissionEndpoint : Endpoint<CreatePermissionRequest, CreatePermissionResponse>
{
    private readonly ApplicationDbContext _db;

    public CreatePermissionEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/permissions");
    }

    public override async Task HandleAsync(CreatePermissionRequest req, CancellationToken ct)
    {
        var perm = new Permission { Name = req.Name };
        _db.Permissions.Add(perm);
        await _db.SaveChangesAsync(ct);
        await SendAsync(new CreatePermissionResponse { Id = perm.Id });
    }
}
