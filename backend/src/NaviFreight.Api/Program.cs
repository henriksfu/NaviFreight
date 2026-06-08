using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NaviFreight.Api.Data;
using NaviFreight.Api.Configuration;
using NaviFreight.Api.Endpoints;
using NaviFreight.Api.Middleware;
using NaviFreight.Api.Repositories;
using NaviFreight.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Options ───────────────────────────────────────────────────────────────────
builder.Services.Configure<TenantOptions>(
    builder.Configuration.GetSection(TenantOptions.SectionName));
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<DataAccessOptions>(
    builder.Configuration.GetSection(DataAccessOptions.SectionName));

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── JWT authentication ────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer   = true,
            ValidIssuer      = jwtSection["Issuer"] ?? "navifreight-api",
            ValidateAudience = true,
            ValidAudience    = jwtSection["Audience"] ?? "navifreight-app",
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.Zero,
            RoleClaimType    = "role",
            NameClaimType    = "email"
        };
    });

// ── Authorization policies ────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantAdmin", p => p.RequireRole("Tenant Admin"));
    options.AddPolicy("DispatchWrite", p => p.RequireRole("Tenant Admin", "Dispatcher"));
    options.AddPolicy("AnyRole",     p => p.RequireAuthenticatedUser());
});

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IAuthRepository, SqlAuthRepository>();
builder.Services.AddScoped<IOperationsRepository, SqlOperationsRepository>();
builder.Services.AddScoped<SqlAuthService>();
builder.Services.AddScoped<SqlOperationsDataService>();
builder.Services.AddSingleton<InMemoryOperationsDataService>();
builder.Services.AddSingleton<InMemoryAuthService>();
builder.Services.AddScoped<IOperationsDataService>(sp =>
{
    var options = sp.GetRequiredService<IOptions<DataAccessOptions>>().Value;
    return string.Equals(options.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase)
        ? sp.GetRequiredService<SqlOperationsDataService>()
        : sp.GetRequiredService<InMemoryOperationsDataService>();
});
builder.Services.AddScoped<IAuthService>(sp =>
{
    var options = sp.GetRequiredService<IOptions<DataAccessOptions>>().Value;
    return string.Equals(options.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase)
        ? sp.GetRequiredService<SqlAuthService>()
        : sp.GetRequiredService<InMemoryAuthService>();
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── Endpoints ─────────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("HealthCheck")
    .AllowAnonymous();

app.MapAuthEndpoints();
app.MapDashboardEndpoints();
app.MapFleetEndpoints();
app.MapYardEndpoints();
app.MapRouteEndpoints();
app.MapAlertEndpoints();
app.MapReportEndpoints();
app.MapSettingsEndpoints();

app.Run();
