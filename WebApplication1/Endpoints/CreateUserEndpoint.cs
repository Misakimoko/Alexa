using FastEndpoints;
using WebApplication1.Data;
using WebApplication1.Entities;

namespace WebApplication1.Endpoints;

public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class CreateUserResponse
{
    public int Id { get; set; }
}

public class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    private readonly ApplicationDbContext _db;

    public CreateUserEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var user = new User
        {
            UserName = req.UserName,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        await SendAsync(new CreateUserResponse { Id = user.Id });
    }
}
