using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

// Notifications personnelles affichées à la prochaine connexion de l'utilisateur.
public class S_Notification
{
    private readonly DAO_Notification _daoNotification;

    public S_Notification(DAO_Notification p_daoNotification)
    {
        _daoNotification = p_daoNotification;
    }

    public async Task Creer(int p_utilisateurId, string p_titre, string p_message, Enum_SeveriteNotification p_severite)
    {
        var _notification = new E_Notification
        {
            UtilisateurId = p_utilisateurId,
            Titre = p_titre,
            Message = p_message,
            Severite = p_severite,
            EstLue = false
        };
        await _daoNotification.Creer(_notification);
    }

    public async Task<List<DTO_Notification>> ObtenirNonLues(int p_utilisateurId)
    {
        var _liste = await _daoNotification.ObtenirNonLues(p_utilisateurId);
        return _liste.Select(n => new DTO_Notification
        {
            Id = n.Id,
            Titre = n.Titre,
            Message = n.Message,
            Severite = n.Severite,
            DateCreation = n.DateCreation
        }).ToList();
    }

    public async Task MarquerLues(int p_utilisateurId)
    {
        await _daoNotification.MarquerToutesLues(p_utilisateurId);
    }
}
