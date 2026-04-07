using System.Text;
using BTPSecure.Server.DAO;
using BTPSecure.Server.Data;
using BTPSecure.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL — Railway fournit DATABASE_URL au format postgresql://user:pass@host:port/db
var _connectionString = ObtenirConnectionString(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(_connectionString));

// JWT — lire depuis les variables d'environnement Railway ou appsettings
var _cleJwt = Environment.GetEnvironmentVariable("JWT_CLE")
    ?? builder.Configuration["Jwt:Cle"]
    ?? "DevSecretKeyMinimum32CaracteresLong!";
var _emetteur = Environment.GetEnvironmentVariable("JWT_EMETTEUR")
    ?? builder.Configuration["Jwt:Emetteur"]
    ?? "BTPSecure";
var _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? "BTPSecure";
var _dureeHeures = Environment.GetEnvironmentVariable("JWT_DUREE_HEURES")
    ?? builder.Configuration["Jwt:DureeHeures"]
    ?? "24";

// Injecter les valeurs JWT dans la configuration pour les services
builder.Configuration["Jwt:Cle"] = _cleJwt;
builder.Configuration["Jwt:Emetteur"] = _emetteur;
builder.Configuration["Jwt:Audience"] = _audience;
builder.Configuration["Jwt:DureeHeures"] = _dureeHeures;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _emetteur,
        ValidAudience = _audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cleJwt))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// DAOs
builder.Services.AddScoped<DAO_Utilisateur>();
builder.Services.AddScoped<DAO_Code>();
builder.Services.AddScoped<DAO_Entreprise>();

// Services
builder.Services.AddScoped<S_Auth>();
builder.Services.AddScoped<S_Code>();
builder.Services.AddScoped<S_Entreprise>();
builder.Services.AddSingleton<S_Pdf>();

var app = builder.Build();

// Healthcheck rapide — Railway ping /health AVANT que la DB soit prête
app.MapGet("/health", () => Results.Ok("ok"));

// Diagnostic BDD — pour vérifier la connexion et les migrations
app.MapGet("/db-status", async (AppDbContext db) =>
{
    try
    {
        var _dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        var _canConnect = await db.Database.CanConnectAsync();
        var _pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        var _appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
        return Results.Ok(new
        {
            connected = _canConnect,
            databaseUrlPresent = !string.IsNullOrEmpty(_dbUrl),
            databaseUrlHost = string.IsNullOrEmpty(_dbUrl) ? "N/A" : new Uri(_dbUrl).Host,
            pending = _pendingMigrations.ToList(),
            applied = _appliedMigrations.ToList()
        });
    }
    catch (Exception ex)
    {
        var _dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        return Results.Ok(new
        {
            connected = false,
            databaseUrlPresent = !string.IsNullOrEmpty(_dbUrl),
            databaseUrlPreview = string.IsNullOrEmpty(_dbUrl) ? "NOT SET" : _dbUrl[..Math.Min(30, _dbUrl.Length)] + "...",
            error = ex.Message
        });
    }
});

// Migration BDD en arrière-plan (ne bloque pas le démarrage)
_ = Task.Run(async () =>
{
    try
    {
        using var _scope = app.Services.CreateScope();
        var _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await _db.Database.MigrateAsync();
        app.Logger.LogInformation("Migration BDD réussie.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erreur migration BDD.");
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

// Convertit DATABASE_URL (format Railway) en connection string Npgsql
static string ObtenirConnectionString(IConfiguration p_config)
{
    var _databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (string.IsNullOrEmpty(_databaseUrl))
        return p_config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=btpsecure;Username=postgres;Password=postgres";

    var _uri = new Uri(_databaseUrl);
    var _userInfo = _uri.UserInfo.Split(':');

    return $"Host={_uri.Host};Port={_uri.Port};Database={_uri.AbsolutePath.TrimStart('/')};Username={_userInfo[0]};Password={_userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;Timeout=5;";
}
