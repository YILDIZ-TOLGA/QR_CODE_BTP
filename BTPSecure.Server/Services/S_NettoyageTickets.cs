using BTPSecure.Server.DAO;

namespace BTPSecure.Server.Services;

// Supprime automatiquement les tickets de plus de 24 h (TTL).
// Tourne toutes les heures et fait aussi un passage au démarrage.
public class S_NettoyageTickets : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<S_NettoyageTickets> _logger;

    private static readonly TimeSpan _intervalle = TimeSpan.FromHours(1);
    private static readonly TimeSpan _dureeVie = TimeSpan.FromHours(24);

    public S_NettoyageTickets(IServiceProvider p_services, ILogger<S_NettoyageTickets> p_logger)
    {
        _services = p_services;
        _logger = p_logger;
    }

    protected override async Task ExecuteAsync(CancellationToken p_stoppingToken)
    {
        while (!p_stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var _scope = _services.CreateScope();
                var _dao = _scope.ServiceProvider.GetRequiredService<DAO_Ticket>();
                var _limite = DateTime.UtcNow.Subtract(_dureeVie);
                var _supprimes = await _dao.SupprimerExpires(_limite);
                if (_supprimes > 0)
                    _logger.LogInformation("Nettoyage tickets : {Nombre} ticket(s) expiré(s) supprimé(s).", _supprimes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du nettoyage des tickets.");
            }

            try
            {
                await Task.Delay(_intervalle, p_stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Arrêt normal de l'application
            }
        }
    }
}
