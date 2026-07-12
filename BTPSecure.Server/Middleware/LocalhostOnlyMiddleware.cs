namespace BTPSecure.Server.Middleware;

/// <summary>
/// Restreint l'accès aux endpoints de diagnostic aux requêtes provenant
/// de localhost uniquement (127.0.0.1 / ::1).
/// Toute requête externe reçoit une réponse 403 Forbidden.
/// NB : "/health" n'est PAS protégé — le healthcheck Railway le ping depuis
/// une IP interne (ex: 100.64.0.2), il doit rester accessible.
/// </summary>
public class LocalhostOnlyMiddleware
{
    private static readonly string[] _protectedPaths =
    [
        "/env-keys",
        "/db-status"
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<LocalhostOnlyMiddleware> _logger;

    public LocalhostOnlyMiddleware(RequestDelegate next, ILogger<LocalhostOnlyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        var isProtected = _protectedPaths.Any(p =>
            path.Equals(p, StringComparison.OrdinalIgnoreCase));

        if (isProtected)
        {
            var remoteIp = context.Connection.RemoteIpAddress;

            // Normalise l'adresse IPv6 mappée en IPv4 (ex: ::ffff:127.0.0.1 → 127.0.0.1)
            if (remoteIp is not null && remoteIp.IsIPv4MappedToIPv6)
                remoteIp = remoteIp.MapToIPv4();

            var isLocalhost = remoteIp is not null
                && (remoteIp.Equals(System.Net.IPAddress.Loopback)      // 127.0.0.1
                    || remoteIp.Equals(System.Net.IPAddress.IPv6Loopback)); // ::1

            if (!isLocalhost)
            {
                _logger.LogWarning(
                    "Accès refusé à {Path} depuis {IP} — endpoint réservé à localhost.",
                    path, remoteIp);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("403 Forbidden — diagnostic endpoints are restricted to localhost.");
                return;
            }
        }

        await _next(context);
    }
}
