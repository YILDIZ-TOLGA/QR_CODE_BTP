using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Server.Services;

public class S_Admin
{
    private readonly DAO_Admin _daoAdmin;
    private readonly S_CacheComptes _cacheComptes;
    private readonly ILogger<S_Admin> _logger;

    public S_Admin(DAO_Admin p_daoAdmin, S_CacheComptes p_cacheComptes, ILogger<S_Admin> p_logger)
    {
        _daoAdmin = p_daoAdmin;
        _cacheComptes = p_cacheComptes;
        _logger = p_logger;
    }

    public async Task<List<DTO_EntrepriseAdmin>> ObtenirToutesLesEntreprises()
    {
        var _entreprises = await _daoAdmin.ObtenirToutesLesEntreprises();
        var _result = new List<DTO_EntrepriseAdmin>();

        foreach (var _e in _entreprises)
        {
            _result.Add(new DTO_EntrepriseAdmin
            {
                Id = _e.Id,
                Nom = _e.Nom,
                Siret = _e.Siret,
                NomDirigeant = _e.Dirigeant.Nom,
                PrenomDirigeant = _e.Dirigeant.Prenom,
                EmailDirigeant = _e.Dirigeant.Email,
                DateCreation = _e.DateCreation,
                EstAutorisee = _e.EstAutorisee,
                NombreCollaborateurs = await _daoAdmin.CompterCollaborateurs(_e.Id),
                NombreCodes = await _daoAdmin.CompterCodes(_e.Id)
            });
        }

        return _result;
    }

    public async Task<List<DTO_FournisseurAdmin>> ObtenirFournisseurs()
    {
        var _fournisseurs = await _daoAdmin.ObtenirFournisseurs();
        var _result = new List<DTO_FournisseurAdmin>();

        foreach (var _f in _fournisseurs)
        {
            var _dto = new DTO_FournisseurAdmin
            {
                Id = _f.Id,
                Nom = _f.Nom,
                Prenom = _f.Prenom,
                NomSociete = _f.NomSociete,
                Email = _f.Email,
                Siret = _f.Siret,
                Siren = _f.Siren,
                Telephone = _f.Telephone,
                DateCreation = _f.DateCreation,
                EstValide = _f.EstValide,
                EstActif = _f.EstActif,
                EstSousCompte = _f.ParentFournisseurId.HasValue,
                LimiteSousComptes = _f.LimiteSousComptes
            };

            // Tout est déjà en mémoire : on évite une requête par fournisseur
            if (_f.ParentFournisseurId.HasValue)
            {
                var _parent = _fournisseurs.FirstOrDefault(u => u.Id == _f.ParentFournisseurId.Value);
                if (_parent != null)
                {
                    _dto.ParentBloque = !_parent.EstActif;
                    _dto.NomParent = $"{_parent.Prenom} {_parent.Nom}".Trim();
                }
            }
            else
            {
                _dto.NombreSousComptes = _fournisseurs.Count(u => u.ParentFournisseurId == _f.Id);
            }

            _result.Add(_dto);
        }

        return _result;
    }

    public async Task<(bool Succes, string Message)> ValiderFournisseur(int p_fournisseurId)
    {
        var _fournisseur = await _daoAdmin.ObtenirUtilisateurParId(p_fournisseurId);
        if (_fournisseur == null || _fournisseur.Role != BTPSecure.Shared.Enums.Enum_Role.Fournisseur)
            return (false, "Fournisseur non trouvé.");

        _fournisseur.EstValide = true;
        _fournisseur.EstActif = true;
        await _daoAdmin.Sauvegarder();
        _cacheComptes.Invalider(_fournisseur.Id);

        _logger.LogInformation("Fournisseur {Email} (ID:{Id}) validé par l'admin.", _fournisseur.Email, _fournisseur.Id);
        return (true, $"Fournisseur {_fournisseur.Nom} validé.");
    }

    // Bloque / débloque un fournisseur. Sur un compte principal, l'action se répercute sur tous ses sous-comptes.
    public async Task<(bool Succes, string Message)> BasculerBlocageFournisseur(int p_fournisseurId)
    {
        var _fournisseur = await _daoAdmin.ObtenirUtilisateurParId(p_fournisseurId);
        if (_fournisseur == null || _fournisseur.Role != BTPSecure.Shared.Enums.Enum_Role.Fournisseur)
            return (false, "Fournisseur non trouvé.");

        bool _nouvelEtat = !_fournisseur.EstActif;

        // Un sous-compte ne peut pas être débloqué tant que son compte principal l'est
        if (_nouvelEtat && _fournisseur.ParentFournisseurId.HasValue)
        {
            var _parent = await _daoAdmin.ObtenirUtilisateurParId(_fournisseur.ParentFournisseurId.Value);
            if (_parent != null && !_parent.EstActif)
                return (false, "Débloquez d'abord le compte principal.");
        }

        _fournisseur.EstActif = _nouvelEtat;

        // Compte principal : la cascade s'applique à tous ses sous-comptes
        int _nbSousComptes = 0;
        var _idsSousComptes = new List<int>();
        if (!_fournisseur.ParentFournisseurId.HasValue)
        {
            var _sousComptes = await _daoAdmin.ObtenirSousComptes(_fournisseur.Id);
            foreach (var _sc in _sousComptes)
            {
                _sc.EstActif = _nouvelEtat;
                _idsSousComptes.Add(_sc.Id);
            }
            _nbSousComptes = _sousComptes.Count;
        }

        await _daoAdmin.Sauvegarder();

        // Le cache doit oublier ces comptes pour que le blocage prenne effet tout de suite
        _cacheComptes.Invalider(_fournisseur.Id);
        foreach (var _idSousCompte in _idsSousComptes)
        {
            _cacheComptes.Invalider(_idSousCompte);
        }

        string _statut;
        if (_nouvelEtat)
        {
            _statut = "débloqué";
        }
        else
        {
            _statut = "bloqué";
        }

        _logger.LogInformation("Fournisseur {Email} (ID:{Id}) {Statut} par l'admin ({Nb} sous-compte(s) impacté(s)).",
            _fournisseur.Email, _fournisseur.Id, _statut, _nbSousComptes);

        if (_nbSousComptes > 0)
            return (true, $"Fournisseur {_statut} ainsi que ses {_nbSousComptes} sous-compte(s).");
        return (true, $"Fournisseur {_statut}.");
    }

    // Change la limite de sous-comptes d'un fournisseur principal (3 par défaut)
    public async Task<(bool Succes, string Message)> ChangerLimiteSousComptes(int p_fournisseurId, int p_limite)
    {
        if (p_limite < 0 || p_limite > 50)
            return (false, "La limite doit être comprise entre 0 et 50.");

        var _fournisseur = await _daoAdmin.ObtenirUtilisateurParId(p_fournisseurId);
        if (_fournisseur == null || _fournisseur.Role != BTPSecure.Shared.Enums.Enum_Role.Fournisseur)
            return (false, "Fournisseur non trouvé.");

        if (_fournisseur.ParentFournisseurId.HasValue)
            return (false, "La limite se règle sur le compte principal, pas sur un sous-compte.");

        _fournisseur.LimiteSousComptes = p_limite;
        await _daoAdmin.Sauvegarder();

        _logger.LogInformation("Limite de sous-comptes de {Email} (ID:{Id}) fixée à {Limite} par l'admin.", _fournisseur.Email, _fournisseur.Id, p_limite);
        return (true, $"Limite fixée à {p_limite} sous-compte(s).");
    }

    public async Task<(bool Succes, string Message)> BasculerAutorisation(int p_entrepriseId)
    {
        var _entreprise = await _daoAdmin.ObtenirEntrepriseParId(p_entrepriseId);
        if (_entreprise == null)
            return (false, "Entreprise non trouvée.");

        _entreprise.EstAutorisee = !_entreprise.EstAutorisee;
        await _daoAdmin.Sauvegarder();

        var _statut = _entreprise.EstAutorisee ? "autorisée" : "bloquée";
        _logger.LogInformation("Entreprise {Nom} (ID:{Id}) {Statut} par l'admin.", _entreprise.Nom, _entreprise.Id, _statut);

        return (true, $"Entreprise {_entreprise.Nom} {_statut}.");
    }
}
