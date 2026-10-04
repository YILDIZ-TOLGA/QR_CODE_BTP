using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

// Suppression définitive d'un compte par un administrateur.
//
// Principe directeur : un admin peut effacer les données D'UN COMPTE, mais pas
// celles d'AUTRUI. Une suppression qui détruirait l'historique d'achat d'une autre
// entreprise, ou la trace d'une commission due, est donc REFUSÉE avec sa raison —
// le compte peut être bloqué à la place, ce qui coupe l'accès sans rien détruire.
public class S_Suppression
{
    private readonly DAO_Suppression _daoSuppression;
    private readonly DAO_Admin _daoAdmin;
    private readonly S_CacheComptes _cacheComptes;
    private readonly ILogger<S_Suppression> _logger;

    public S_Suppression(DAO_Suppression p_daoSuppression, DAO_Admin p_daoAdmin,
        S_CacheComptes p_cacheComptes, ILogger<S_Suppression> p_logger)
    {
        _daoSuppression = p_daoSuppression;
        _daoAdmin = p_daoAdmin;
        _cacheComptes = p_cacheComptes;
        _logger = p_logger;
    }

    // Ce que la suppression détruirait, et ce qui l'empêche.
    // Calculé avant toute proposition : l'admin doit décider en connaissance de cause.
    public async Task<DTO_ApercuSuppression?> ObtenirApercu(int p_cibleId, int p_adminId)
    {
        var _cible = await _daoAdmin.ObtenirUtilisateurParId(p_cibleId);
        if (_cible == null)
            return null;

        var _apercu = new DTO_ApercuSuppression
        {
            UtilisateurId = _cible.Id,
            NomComplet = (_cible.Prenom + " " + _cible.Nom).Trim(),
            Email = _cible.Email,
            Role = _cible.Role.ToString()
        };

        // ---------- Blocages ----------

        if (_cible.Id == p_adminId)
        {
            _apercu.Blocages.Add("Vous ne pouvez pas supprimer votre propre compte.");
        }

        if (_cible.Role == Enum_Role.Admin)
        {
            _apercu.Blocages.Add("Les comptes administrateurs ne sont pas supprimables depuis cette interface (risque de perdre tout accès à l'administration).");
        }

        var _validateur = await _daoSuppression.CompterValidationsCommeValidateur(_cible.Id);
        if (_validateur > 0)
        {
            _apercu.Blocages.Add($"Ce compte a validé {_validateur} commande(s) : les supprimer effacerait l'historique d'achat d'autres entreprises.");
        }

        var _porteur = await _daoSuppression.CompterValidationsCommePorteur(_cible.Id);
        if (_porteur > 0)
        {
            _apercu.Blocages.Add($"{_porteur} validation(s) sont enregistrées à son nom : cet historique appartient à son entreprise.");
        }

        var _parrainagesValides = await _daoSuppression.CompterParrainagesValides(_cible.Id);
        if (_parrainagesValides > 0)
        {
            _apercu.Blocages.Add($"{_parrainagesValides} parrainage(s) avec commission déjà validée : c'est une trace financière, elle ne doit pas disparaître.");
        }

        var _sousComptes = await _daoSuppression.CompterSousComptes(_cible.Id);
        if (_sousComptes > 0)
        {
            _apercu.Blocages.Add($"Ce fournisseur a {_sousComptes} sous-compte(s). Supprimez-les d'abord, ou bloquez le compte principal.");
        }

        // ---------- Étendue de la suppression ----------

        var _entreprise = await _daoSuppression.ObtenirEntrepriseDuDirigeant(_cible.Id);
        if (_entreprise != null)
        {
            var _validationsEntreprise = await _daoSuppression.CompterValidationsDeLEntreprise(_entreprise.Id);
            if (_validationsEntreprise > 0)
            {
                _apercu.Blocages.Add($"Son entreprise « {_entreprise.Nom} » compte {_validationsEntreprise} validation(s) dans son historique : supprimer le dirigeant détruirait cet historique.");
            }

            _apercu.NomEntreprise = _entreprise.Nom;
            _apercu.NombreCollaborateursDetaches = await _daoAdmin.CompterCollaborateurs(_entreprise.Id);
        }

        _apercu.NombreCodes = await _daoSuppression.CompterCodesLies(_cible.Id);
        _apercu.NombreMessages = await _daoSuppression.CompterMessages(_cible.Id);
        _apercu.NombreMemos = await _daoSuppression.CompterMemos(_cible.Id);
        _apercu.NombreNotifications = await _daoSuppression.CompterNotifications(_cible.Id);
        _apercu.NombreLiensEntreprise = await _daoSuppression.CompterLiensEntreprise(_cible.Id);
        _apercu.NombreContactsFournisseur = await _daoSuppression.CompterContactsFournisseur(_cible.Id);
        _apercu.NombreBlacklist = await _daoSuppression.CompterBlacklist(_cible.Id);
        _apercu.NombreParrainagesEnAttente = await _daoSuppression.CompterParrainagesEnAttente(_cible.Id);

        _apercu.EstSupprimable = _apercu.Blocages.Count == 0;
        return _apercu;
    }

    // Supprime pour de bon.
    // p_emailConfirme : l'admin doit avoir retapé l'email du compte. Ce n'est pas une
    // formalité — c'est ce qui empêche de supprimer la mauvaise ligne d'une liste.
    public async Task<(bool Succes, string Message)> Supprimer(int p_cibleId, int p_adminId, string? p_emailConfirme)
    {
        // On recalcule les blocages côté serveur : l'aperçu affiché a pu vieillir,
        // et un contrôle fait seulement dans le navigateur ne protège rien.
        var _apercu = await ObtenirApercu(p_cibleId, p_adminId);
        if (_apercu == null)
            return (false, "Compte introuvable.");

        if (!_apercu.EstSupprimable)
            return (false, _apercu.Blocages[0]);

        var _saisie = string.Empty;
        if (p_emailConfirme != null)
        {
            _saisie = p_emailConfirme.Trim().ToLower();
        }
        if (_saisie != _apercu.Email.Trim().ToLower())
            return (false, "L'email saisi ne correspond pas à celui du compte à supprimer.");

        var _erreur = await _daoSuppression.Supprimer(p_cibleId);
        if (_erreur != null)
        {
            _logger.LogError("Suppression du compte {Id} ({Email}) echouee : {Erreur}", p_cibleId, _apercu.Email, _erreur);
            return (false, "La suppression a échoué et a été entièrement annulée : " + _erreur);
        }

        // ⚠️ Sans cette invalidation, le jeton du compte supprimé resterait accepté
        // jusqu'à l'expiration du cache (5 min).
        _cacheComptes.Invalider(p_cibleId);

        _logger.LogWarning("Compte {Id} ({Email}, {Role}) SUPPRIME definitivement par l'admin {Admin}",
            p_cibleId, _apercu.Email, _apercu.Role, p_adminId);

        return (true, $"Le compte « {_apercu.Email} » a été supprimé définitivement.");
    }
}
