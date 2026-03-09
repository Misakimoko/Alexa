using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Lexa.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFastEndpoints();
// Use NSwag/OpenAPI via FastEndpoints.Swagger package
builder.Services.AddOpenApiDocument();

builder.Services.AutoRegister();


// Configure EF Core DbContext with provider selected from configuration
// Supported providers: SqlServer | PostgreSQL | MySql | Oracle | Sqlite
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var config = builder.Configuration;
    var provider = config["Database:Provider"] ?? string.Empty;
    var conn = config.GetConnectionString("DefaultConnection") ?? config["Database:ConnectionString"] ?? "Data Source=app.db";

    switch (provider)
    {
        case "SqlServer":
            options.UseSqlServer(conn);
            break;
        case "PostgreSQL":
            options.UseNpgsql(conn);
            break;
        case "MySql":
            // Requires Pomelo.EntityFrameworkCore.MySql or equivalent provider package
            options.UseMySql(conn, ServerVersion.AutoDetect(conn));
            break;
        default:
            // Default to Sqlite for quick local runs
            options.UseSqlite(conn);
            break;
    }
});

//添加sqlsugarscop依赖注入
builder.Services.AddScoped<ISqlSugarClient>(sp =>
{
    var config = builder.Configuration;
    var provider = config["Database:Provider"] ?? string.Empty;
    var conn = config.GetConnectionString("DefaultConnection") ?? config["Database:ConnectionString"] ?? "Data Source=app.db";
    var dbType = provider switch
    {
        "SqlServer" => DbType.SqlServer,
        "PostgreSQL" => DbType.PostgreSQL,
        "MySql" => DbType.MySql,
        _ => DbType.Sqlite
    };
    return new SqlSugarClient(new ConnectionConfig
    {
        ConnectionString = conn,
        DbType = dbType,
        IsAutoCloseConnection = true
    });
});



// configure authentication (JWT)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "please-change-this-secret";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "myapp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "myapp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();
// register token service
builder.Services.AddScoped<Lexa.Security.ITokenService, Lexa.Security.TokenService>();

// auto-register services marked with AutoregisterInject attributes
var app = builder.Build();

// run seed data (create admin user / roles / permissions)
try
{
    SeedData.SeedAsync(app.Services).GetAwaiter().GetResult();
}
catch (Exception ex)
{
    // don't prevent app from starting if seeding fails; log to console
    Console.WriteLine($"Seed error: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

app.Run();
