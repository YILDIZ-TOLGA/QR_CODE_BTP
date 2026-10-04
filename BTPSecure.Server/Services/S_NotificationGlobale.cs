using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

// Diffusions d'un administrateur vers un ou plusieurs rôles, affichées à la connexion
// des destinataires pendant une fenêtre de dates.
public class S_NotificationGlobale
{
    private readonly DAO_NotificationGlobale _dao;
    private readonly ILogger<S_NotificationGlobale> _logger;

    // Rôles que l'admin peut viser. Écrits en dur plutôt que déduits de l'enum par
    // réflexion (pièges du trimmer), et sert aussi de liste blanche à la saisie.
    private static readonly string[] c_rolesCiblables = new[]
    {
        "Dirigeant", "Collaborateur", "Fournisseur", "ApporteurAffaire"
    };

    public S_NotificationGlobale(DAO_NotificationGlobale p_dao, ILogger<S_NotificationGlobale> p_logger)
    {
        _dao = p_dao;
        _logger = p_logger;
    }

    public static List<string> RolesCiblables()
    {
        return c_rolesCiblables.ToList();
    }

    public async Task<(bool Succes, string Message)> Creer(DTO_CreerNotificationGlobale p_dto, int p_adminId)
    {
        if (string.IsNullOrWhiteSpace(p_dto.Titre))
            return (false, "Le titre est obligatoire.");
        if (string.IsNullOrWhiteSpace(p_dto.Message))
            return (false, "Le message est obligatoire.");
        if (p_dto.Titre.Trim().Length > 150)
            return (false, "Le titre ne doit pas dépasser 150 caractères.");
        if (p_dto.Message.Trim().Length > 2000)
            return (false, "Le message ne doit pas dépasser 2000 caractères.");

        // Liste blanche : un rôle inconnu ne doit pas entrer en base, il ne
        // correspondrait à personne et resterait là sans qu'on comprenne pourquoi.
        var _roles = new List<string>();
        foreach (var _role in p_dto.RolesCibles)
        {
            if (_role == null)
                continue;
            var _propre = _role.Trim();
            if (!c_rolesCiblables.Contains(_propre))
                return (false, $"Rôle inconnu : « {_propre} ».");
            if (!_roles.Contains(_propre))
            {
                _roles.Add(_propre);
            }
        }

        if (_roles.Count == 0)
            return (false, "Choisissez au moins un rôle destinataire.");

        var _debut = EnUtc(p_dto.DateDebut);
        var _fin = EnUtc(p_dto.DateFin);

        if (_fin <= _debut)
            return (false, "La date de fin doit être postérieure à la date de début.");

        var _notification = new E_NotificationGlobale
        {
            Titre = p_dto.Titre.Trim(),
            Message = p_dto.Message.Trim(),
            Severite = p_dto.Severite,
            RolesCibles = string.Join(",", _roles),
            DateDebut = _debut,
            DateFin = _fin,
            EstActive = true,
            DateCreation = DateTime.UtcNow,
            CreateurId = p_adminId
        };

        await _dao.Creer(_notification);
        _logger.LogInformation("Diffusion {Id} creee par l'admin {Admin} pour {Roles}, du {Debut} au {Fin}",
            _notification.Id, p_adminId, _notification.RolesCibles, _debut, _fin);

        return (true, "Notification programmée.");
    }

    public async Task<List<DTO_NotificationGlobaleAdmin>> ObtenirToutes()
    {
        var _maintenant = DateTime.UtcNow;
        var _liste = await _dao.ObtenirToutes();
        var _result = new List<DTO_NotificationGlobaleAdmin>();

        foreach (var _n in _liste)
        {
            var _roles = DecouperRoles(_n.RolesCibles);

            var _rolesEnum = new List<Enum_Role>();
            foreach (var _role in _roles)
            {
                Enum_Role _valeur;
                if (Enum.TryParse(_role, out _valeur))
                {
                    _rolesEnum.Add(_valeur);
                }
            }

            _result.Add(new DTO_NotificationGlobaleAdmin
            {
                Id = _n.Id,
                Titre = _n.Titre,
                Message = _n.Message,
                Severite = _n.Severite,
                RolesCibles = _roles,
                DateDebut = _n.DateDebut,
                DateFin = _n.DateFin,
                EstActive = _n.EstActive,
                DateCreation = _n.DateCreation,
                Etat = CalculerEtat(_n, _maintenant),
                NombreVues = await _dao.CompterVues(_n.Id),
                NombreDestinataires = await _dao.CompterDestinataires(_rolesEnum)
            });
        }

        return _result;
    }

    public async Task<(bool Succes, string Message)> BasculerActivation(int p_id)
    {
        var _notification = await _dao.ObtenirParId(p_id);
        if (_notification == null)
            return (false, "Notification introuvable.");

        _notification.EstActive = !_notification.EstActive;
        await _dao.Sauvegarder();

        if (_notification.EstActive)
            return (true, "Notification réactivée.");
        return (true, "Notification désactivée : elle ne s'affichera plus.");
    }

    public async Task<(bool Succes, string Message)> Supprimer(int p_id)
    {
        var _notification = await _dao.ObtenirParId(p_id);
        if (_notification == null)
            return (false, "Notification introuvable.");

        await _dao.Supprimer(p_id);
        _logger.LogInformation("Diffusion {Id} supprimee", p_id);
        return (true, "Notification supprimée.");
    }

    // Diffusions à afficher à cet utilisateur : actives, dans la fenêtre, visant son rôle.
    // ⚠️ On ne filtre PAS sur « déjà vue » : le message doit revenir à CHAQUE connexion
    // tant que la période court. La table des vues ne sert donc qu'à mesurer la portée
    // (combien de destinataires distincts l'ont vue au moins une fois).
    // Pas de risque d'affichage en boucle : MainLayout n'interroge les notifications
    // qu'une fois par session (garde `_notificationsVerifiees`).
    public async Task<List<E_NotificationGlobale>> ObtenirPourUtilisateur(int p_utilisateurId, Enum_Role p_role)
    {
        var _actives = await _dao.ObtenirActives(DateTime.UtcNow);
        if (_actives.Count == 0)
            return new List<E_NotificationGlobale>();

        var _role = p_role.ToString();
        var _result = new List<E_NotificationGlobale>();

        foreach (var _n in _actives)
        {
            if (!DecouperRoles(_n.RolesCibles).Contains(_role))
                continue;
            _result.Add(_n);
        }

        return _result;
    }

    public async Task MarquerVues(int p_utilisateurId, List<int> p_ids)
    {
        await _dao.MarquerVues(p_utilisateurId, p_ids);
    }

    private static List<string> DecouperRoles(string p_roles)
    {
        var _result = new List<string>();
        if (string.IsNullOrWhiteSpace(p_roles))
            return _result;

        foreach (var _morceau in p_roles.Split(','))
        {
            var _propre = _morceau.Trim();
            if (_propre.Length > 0)
            {
                _result.Add(_propre);
            }
        }
        return _result;
    }

    private static string CalculerEtat(E_NotificationGlobale p_n, DateTime p_maintenant)
    {
        if (!p_n.EstActive)
        {
            return "Desactivee";
        }
        if (p_maintenant < p_n.DateDebut)
        {
            return "AVenir";
        }
        if (p_maintenant > p_n.DateFin)
        {
            return "Terminee";
        }
        return "EnCours";
    }

    // PostgreSQL (`timestamp with time zone`) refuse un DateTime dont le Kind est
    // Unspecified. Le navigateur envoie déjà de l'UTC ; ce filet couvre les autres cas
    // sans jamais réinterpréter une heure selon le fuseau du serveur Railway, qui n'a
    // rien à voir avec celui de l'admin.
    private static DateTime EnUtc(DateTime p_date)
    {
        if (p_date.Kind == DateTimeKind.Utc)
        {
            return p_date;
        }
        if (p_date.Kind == DateTimeKind.Local)
        {
            return p_date.ToUniversalTime();
        }
        return DateTime.SpecifyKind(p_date, DateTimeKind.Utc);
    }
}
