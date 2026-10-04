using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

// Notifications personnelles affichées à la prochaine connexion de l'utilisateur.
public class S_Notification
{
    private readonly DAO_Notification _daoNotification;
    private readonly DAO_Utilisateur _daoUtilisateur;
    private readonly S_NotificationGlobale _sGlobale;

    public S_Notification(DAO_Notification p_daoNotification, DAO_Utilisateur p_daoUtilisateur,
        S_NotificationGlobale p_sGlobale)
    {
        _daoNotification = p_daoNotification;
        _daoUtilisateur = p_daoUtilisateur;
        _sGlobale = p_sGlobale;
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

    // Notifications personnelles ET diffusions globales destinees a son role.
    // Un seul point d'accroche : l'affichage en toast (MainLayout) n'a pas a connaitre
    // la difference, et il n'y a pas deux chemins a maintenir.
    public async Task<List<DTO_Notification>> ObtenirNonLues(int p_utilisateurId)
    {
        var _liste = await _daoNotification.ObtenirNonLues(p_utilisateurId);
        var _result = _liste.Select(n => new DTO_Notification
        {
            Id = n.Id,
            Titre = n.Titre,
            Message = n.Message,
            Severite = n.Severite,
            DateCreation = n.DateCreation
        }).ToList();

        foreach (var _g in await ObtenirGlobalesNonVues(p_utilisateurId))
        {
            _result.Add(new DTO_Notification
            {
                Id = _g.Id,
                Titre = _g.Titre,
                Message = _g.Message,
                Severite = _g.Severite,
                DateCreation = _g.DateCreation
            });
        }

        return _result;
    }

    public async Task MarquerLues(int p_utilisateurId)
    {
        await _daoNotification.MarquerToutesLues(p_utilisateurId);

        // Les diffusions ne sont pas « lues » mais « vues » : sans cette trace, le message
        // reviendrait a chaque connexion pendant toute sa fenetre de dates.
        var _globales = await ObtenirGlobalesNonVues(p_utilisateurId);
        if (_globales.Count == 0)
            return;

        var _ids = new List<int>();
        foreach (var _g in _globales)
        {
            _ids.Add(_g.Id);
        }
        await _sGlobale.MarquerVues(p_utilisateurId, _ids);
    }

    private async Task<List<E_NotificationGlobale>> ObtenirGlobalesNonVues(int p_utilisateurId)
    {
        var _utilisateur = await _daoUtilisateur.ObtenirParId(p_utilisateurId);
        if (_utilisateur == null)
            return new List<E_NotificationGlobale>();

        return await _sGlobale.ObtenirPourUtilisateur(p_utilisateurId, _utilisateur.Role);
    }
}
