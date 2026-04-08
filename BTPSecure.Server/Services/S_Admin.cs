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
                NomPatron = _e.Patron.Nom,
                PrenomPatron = _e.Patron.Prenom,
                EmailPatron = _e.Patron.Email,
                DateCreation = _e.DateCreation,
                EstAutorisee = _e.EstAutorisee,
                NombreSalaries = await _daoAdmin.CompterSalaries(_e.Id),
                NombreCodes = await _daoAdmin.CompterCodes(_e.Id)
            });
        }

        return _result;
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
