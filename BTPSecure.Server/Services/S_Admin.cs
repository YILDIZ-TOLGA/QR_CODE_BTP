using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Server.Services;

public class S_Admin
{
    private readonly DAO_Admin _daoAdmin;
    private readonly ILogger<S_Admin> _logger;

    public S_Admin(DAO_Admin p_daoAdmin, ILogger<S_Admin> p_logger)
    {
        _daoAdmin = p_daoAdmin;
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
        return _fournisseurs.Select(f => new DTO_FournisseurAdmin
        {
            Id = f.Id,
            Nom = f.Nom,
            Prenom = f.Prenom,
            Email = f.Email,
            Siret = f.Siret,
            Siren = f.Siren,
            Telephone = f.Telephone,
            DateCreation = f.DateCreation,
            EstValide = f.EstValide,
            EstActif = f.EstActif
        }).ToList();
    }

    public async Task<(bool Succes, string Message)> ValiderFournisseur(int p_fournisseurId)
    {
        var _fournisseur = await _daoAdmin.ObtenirUtilisateurParId(p_fournisseurId);
        if (_fournisseur == null || _fournisseur.Role != BTPSecure.Shared.Enums.Enum_Role.Fournisseur)
            return (false, "Fournisseur non trouvé.");

        _fournisseur.EstValide = true;
        _fournisseur.EstActif = true;
        await _daoAdmin.Sauvegarder();

        _logger.LogInformation("Fournisseur {Email} (ID:{Id}) validé par l'admin.", _fournisseur.Email, _fournisseur.Id);
        return (true, $"Fournisseur {_fournisseur.Nom} validé.");
    }

    public async Task<(bool Succes, string Message)> DesactiverFournisseur(int p_fournisseurId)
    {
        var _fournisseur = await _daoAdmin.ObtenirUtilisateurParId(p_fournisseurId);
        if (_fournisseur == null || _fournisseur.Role != BTPSecure.Shared.Enums.Enum_Role.Fournisseur)
            return (false, "Fournisseur non trouvé.");

        _fournisseur.EstActif = false;
        await _daoAdmin.Sauvegarder();

        _logger.LogInformation("Fournisseur {Email} (ID:{Id}) désactivé par l'admin.", _fournisseur.Email, _fournisseur.Id);
        return (true, $"Fournisseur {_fournisseur.Nom} désactivé.");
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
