using FastEndpoints;
using System.Threading;

namespace WebApplication1.Endpoints;

public class HelloRequest
{
    public string? Name { get; set; }
}

public class HelloResponse
{
    public string Message { get; set; } = string.Empty;
}

public class HelloEndpoint : Endpoint<HelloRequest, HelloResponse>
{
    public override void Configure()
    {
        Post("/hello");
        AllowAnonymous();
    }

    public override async Task HandleAsync(HelloRequest req, CancellationToken ct)
    {
        var name = string.IsNullOrWhiteSpace(req.Name) ? "world" : req.Name;
        var resp = new HelloResponse { Message = $"Hello {name}!" };
        await SendAsync(resp);
    }
}
