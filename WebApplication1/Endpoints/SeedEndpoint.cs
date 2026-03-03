using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using WebApplication1.Data;
using WebApplication1.Authorization;

namespace WebApplication1.Endpoints;

public class SeedResponse
{
    public string Message { get; set; } = string.Empty;
}

public class SeedEndpoint : EndpointWithoutRequest<SeedResponse>
{
    private readonly IWebHostEnvironment _env;
    private readonly IAuthorizationService _auth;
    private readonly IServiceProvider _services;

    public SeedEndpoint(IWebHostEnvironment env, IAuthorizationService auth, IServiceProvider services)
    {
        _env = env;
        _auth = auth;
        _services = services;
    }

    public override void Configure()
    {
        Post("/api/admin/seed");
        // allow anonymous here; method will enforce stricter checks in non-development
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Allow automatic seeding in Development without auth, otherwise require permission
        if (!_env.IsDevelopment())
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                await SendForbiddenAsync(ct);
                return;
            }

            var authorized = await _auth.AuthorizeAsync(User, null, new PermissionRequirement("role:manage"));
            if (!authorized.Succeeded)
            {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        try
        {
            await SeedData.SeedAsync(_services);
            await SendAsync(new SeedResponse { Message = "Seed completed." }, cancellation: ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new SeedResponse { Message = $"Seed failed: {ex.Message}" }, cancellation: ct);
        }
    }
}
