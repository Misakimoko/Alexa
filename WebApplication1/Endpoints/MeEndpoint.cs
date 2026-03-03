using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using WebApplication1.Data;
using WebApplication1.Entities;

namespace WebApplication1.Endpoints;

[Authorize]
public class MeResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
}

[Authorize]
public class MeEndpoint : EndpointWithoutRequest<MeResponse>
{
    private readonly ApplicationDbContext _db;

    public MeEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/me");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sub = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !int.TryParse(sub, out var userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var resp = new MeResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email
        };

        await SendAsync(resp, cancellation: ct);
    }
}
