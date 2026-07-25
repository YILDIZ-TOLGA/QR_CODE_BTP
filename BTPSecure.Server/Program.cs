using System.Text;
using BTPSecure.Server.DAO;
using BTPSecure.Server.Data;
using BTPSecure.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

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

// Compression gzip/brotli — réduit la taille des fichiers WASM de 60-70 %
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/octet-stream",   // .wasm
        "application/wasm",
        "text/javascript",
        "application/javascript",
        "text/css",
        "font/woff2",
        "image/svg+xml"
    });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o =>
    o.Level = System.IO.Compression.CompressionLevel.Optimal);
builder.Services.Configure<GzipCompressionProviderOptions>(o =>
    o.Level = System.IO.Compression.CompressionLevel.Optimal);

// DAOs
builder.Services.AddScoped<DAO_Utilisateur>();
builder.Services.AddScoped<DAO_Code>();
builder.Services.AddScoped<DAO_Entreprise>();
builder.Services.AddScoped<DAO_Admin>();
builder.Services.AddScoped<DAO_FournisseurContact>();
builder.Services.AddScoped<DAO_Blacklist>();
builder.Services.AddScoped<DAO_Ticket>();
builder.Services.AddScoped<DAO_Memo>();

// Services
builder.Services.AddScoped<S_Auth>();
builder.Services.AddScoped<S_Code>();
builder.Services.AddScoped<S_Entreprise>();
builder.Services.AddScoped<S_Admin>();
builder.Services.AddScoped<S_Invitation>();
builder.Services.AddScoped<S_FournisseurContact>();
builder.Services.AddScoped<S_SousCompte>();
builder.Services.AddScoped<S_Blacklist>();
builder.Services.AddScoped<S_Ticket>();
builder.Services.AddScoped<S_Memo>();
builder.Services.AddSingleton<S_Pdf>();
builder.Services.AddSingleton<S_Email>();

// Nettoyage automatique des tickets expirés (TTL 24 h)
builder.Services.AddHostedService<S_NettoyageTickets>();

var app = builder.Build();

// Restreint /health, /env-keys et /db-status à localhost uniquement
app.UseMiddleware<BTPSecure.Server.Middleware.LocalhostOnlyMiddleware>();

// Healthcheck rapide — Railway ping /health AVANT que la DB soit prête
app.MapGet("/health", () => Results.Ok("ok"));

// Debug: liste les noms de toutes les variables d'environnement
app.MapGet("/env-keys", () =>
{
    var _vars = Environment.GetEnvironmentVariables();
    var _keys = new List<string>();
    foreach (System.Collections.DictionaryEntry entry in _vars)
        _keys.Add(entry.Key?.ToString() ?? "");
    _keys.Sort();
    return Results.Ok(new { count = _keys.Count, keys = _keys });
});

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

// Migration BDD + Seed Admin en arrière-plan (ne bloque pas le démarrage)
_ = Task.Run(async () =>
{
    try
    {
        using var _scope = app.Services.CreateScope();
        var _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await _db.Database.MigrateAsync();
        app.Logger.LogInformation("Migration BDD réussie.");

        // Seed compte Admin s'il n'existe pas. Identifiants lus depuis les variables
        // d'environnement Railway : le mot de passe n'est JAMAIS en dur dans le code.
        // EmailVerifie/EstValide à true : sinon la connexion serait bloquée (cf. S_Auth.Connecter).
        if (!await _db.Utilisateurs.AnyAsync(u => u.Role == BTPSecure.Shared.Enums.Enum_Role.Admin))
        {
            var _adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            if (string.IsNullOrWhiteSpace(_adminEmail))
                _adminEmail = "admin_acc@keydopro.com";
            var _adminMotDePasse = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrWhiteSpace(_adminMotDePasse))
            {
                app.Logger.LogWarning("ADMIN_PASSWORD non défini : compte admin NON créé. Ajoute la variable sur Railway puis redémarre.");
            }
            else
            {
                var _sel = BCrypt.Net.BCrypt.GenerateSalt();
                _db.Utilisateurs.Add(new BTPSecure.Shared.Entites.E_Utilisateur
                {
                    Email = _adminEmail.Trim().ToLower(),
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(_adminMotDePasse, _sel),
                    Sel = _sel,
                    Nom = "Admin",
                    Prenom = "KEYDO",
                    Role = BTPSecure.Shared.Enums.Enum_Role.Admin,
                    EstActif = true,
                    EstValide = true,
                    EmailVerifie = true
                });
                await _db.SaveChangesAsync();
                app.Logger.LogInformation("Compte Admin créé : {Email}", _adminEmail);
            }
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erreur migration/seed BDD.");
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

// Compression doit être placée avant les fichiers statiques
app.UseResponseCompression();

// ── Cache-Control pour les assets statiques ──────────────────────────────────
// Les fichiers Blazor WASM portent un fingerprint dans leur nom (ex: blazor.wasm.abc123)
// → on peut les mettre en cache très longtemps (1 an, immutable).
// Les fichiers HTML ne sont jamais versionnés → cache court (1 h) pour permettre les mises à jour.

var _contentTypeProvider = new FileExtensionContentTypeProvider();

app.UseBlazorFrameworkFiles();

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = _contentTypeProvider,
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name;
        var headers = ctx.Context.Response.GetTypedHeaders();

        // Assets versionnés (WASM, JS, CSS, polices) → cache 1 an, immutable
        if (path.EndsWith(".wasm", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".js",    StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".css",   StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".woff",  StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".ttf",   StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".svg",   StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".png",   StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".ico",   StringComparison.OrdinalIgnoreCase))
        {
            headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(365),
                Extensions = { new NameValueHeaderValue("immutable") }
            };
        }
        // HTML → cache court pour permettre les déploiements
        else if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromHours(1)
            };
        }
    }
});

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
