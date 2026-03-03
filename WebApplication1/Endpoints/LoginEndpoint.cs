using FastEndpoints;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Entities;
using WebApplication1.Security;

namespace WebApplication1.Endpoints;

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly ITokenService _tokenSvc;

    public LoginEndpoint(ApplicationDbContext db, IConfiguration config, ITokenService tokenSvc)
    {
        _db = db;
        _config = config;
        _tokenSvc = tokenSvc;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == req.UserName, ct);
        if (user == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var verified = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!verified)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var (tokenStr, expires) = await _tokenSvc.CreateTokenAsync(user, ct);
        await SendAsync(new LoginResponse { Token = tokenStr, ExpiresAt = expires });
    }
}
