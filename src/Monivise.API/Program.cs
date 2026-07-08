using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Monivise.Infrastructure;
using Monivise.API.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Controllers + JSON ───
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ─── JWT Authentication ───
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ─── Infrastructure ───
builder.Services.AddInfrastructure(builder.Configuration);

// ─── OpenAPI ───
builder.Services.AddOpenApi();

// ─── CORS ───
builder.Services.AddCors(opts => opts.AddPolicy("Dev", policy =>
    policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",
            "http://localhost:5259",
            "https://localhost:7160",
            "http://localhost:5046")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

// ─── Build ───
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                  // JSON spec at /openapi/v1.json
    app.MapScalarApiReference();       // UI at /scalar/v1
}

app.UseHttpsRedirection();
app.UseCors("Dev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Monivise.Infrastructure.Data.AppDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();