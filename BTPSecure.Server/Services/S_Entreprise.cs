using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

public class S_Entreprise
{
    private readonly DAO_Entreprise _daoEntreprise;
    private readonly DAO_Utilisateur _daoUtilisateur;
    private readonly DAO_Code _daoCode;
    private readonly ILogger<S_Entreprise> _logger;

    public S_Entreprise(DAO_Entreprise p_daoEntreprise, DAO_Utilisateur p_daoUtilisateur,
        DAO_Code p_daoCode, ILogger<S_Entreprise> p_logger)
    {
        _daoEntreprise = p_daoEntreprise;
        _daoUtilisateur = p_daoUtilisateur;
        _daoCode = p_daoCode;
        _logger = p_logger;
    }

    public async Task<(bool Succes, string Message, DTO_EntrepriseAffichage? Entreprise)> Creer(DTO_CreerEntreprise p_dto, int p_patronId)
    {
        if (string.IsNullOrWhiteSpace(p_dto.Nom))
            return (false, "Le nom de l'entreprise est obligatoire.", null);

        var _existante = await _daoEntreprise.ObtenirParPatronId(p_patronId);
        if (_existante != null)
            return (false, "Vous avez déjà une entreprise.", null);

        var _entreprise = new E_Entreprise
        {
            Nom = p_dto.Nom.Trim(),
            Adresse = p_dto.Adresse?.Trim(),
            Siret = p_dto.Siret?.Trim(),
            PatronId = p_patronId
        };

        await _daoEntreprise.Creer(_entreprise);
        _logger.LogInformation("Entreprise '{Nom}' créée par patron {PatronId}", _entreprise.Nom, p_patronId);

        return (true, "Entreprise créée avec succès.", VersDTO(_entreprise));
    }

    public async Task<DTO_EntrepriseAffichage?> ObtenirParPatron(int p_patronId)
    {
        var _entreprise = await _daoEntreprise.ObtenirParPatronId(p_patronId);
        return _entreprise == null ? null : VersDTO(_entreprise);
    }

    public async Task<(bool Succes, string Message, DTO_SalarieAffichage? Salarie)> AjouterSalarie(string p_email, int p_patronId)
    {
        if (string.IsNullOrWhiteSpace(p_email))
            return (false, "L'email est obligatoire.", null);

        var _entreprise = await _daoEntreprise.ObtenirParPatronId(p_patronId);
        if (_entreprise == null)
            return (false, "Vous n'avez pas encore d'entreprise.", null);

        var _salarie = await _daoUtilisateur.ObtenirParEmail(p_email);
        if (_salarie == null)
            return (false, "Aucun utilisateur trouvé avec cet email.", null);

        if (_salarie.Role != Enum_Role.Salarie)
            return (false, "Cet utilisateur n'a pas le rôle Salarié.", null);

        if (await _daoEntreprise.SalarieEstDansEntreprise(_salarie.Id, _entreprise.Id))
            return (false, "Ce salarié est déjà dans votre entreprise.", null);

        var _lien = new E_SalarieEntreprise
        {
            SalarieId = _salarie.Id,
            EntrepriseId = _entreprise.Id
        };

        await _daoEntreprise.AjouterSalarie(_lien);
        _logger.LogInformation("Salarié {Email} ajouté à l'entreprise {EntrepriseId}", p_email, _entreprise.Id);

        return (true, "Salarié ajouté avec succès.", new DTO_SalarieAffichage
        {
            Id = _lien.Id,
            SalarieId = _salarie.Id,
            Nom = _salarie.Nom,
            Prenom = _salarie.Prenom,
            Email = _salarie.Email,
            DateAjout = _lien.DateAjout
        });
    }

    public async Task<List<DTO_SalarieAffichage>> ObtenirSalaries(int p_patronId)
    {
        var _entreprise = await _daoEntreprise.ObtenirParPatronId(p_patronId);
        if (_entreprise == null) return new List<DTO_SalarieAffichage>();

        var _liens = await _daoEntreprise.ObtenirSalaries(_entreprise.Id);
        return _liens.Select(l => new DTO_SalarieAffichage
        {
            Id = l.Id,
            SalarieId = l.SalarieId,
            Nom = l.Salarie.Nom,
            Prenom = l.Salarie.Prenom,
            Email = l.Salarie.Email,
            DateAjout = l.DateAjout
        }).ToList();
    }

    public async Task<(bool Succes, string Message)> RetirerSalarie(int p_lienId, int p_patronId)
    {
        var _entreprise = await _daoEntreprise.ObtenirParPatronId(p_patronId);
        if (_entreprise == null)
            return (false, "Vous n'avez pas d'entreprise.");

        var _liens = await _daoEntreprise.ObtenirSalaries(_entreprise.Id);
        var _lien = _liens.FirstOrDefault(l => l.Id == p_lienId);
        if (_lien == null)
            return (false, "Ce salarié n'est pas dans votre entreprise.");

        await _daoEntreprise.RetirerSalarie(_lien);
        await _daoCode.RevoquerCodesParSalarieEtEntreprise(_lien.SalarieId, _entreprise.Id);

        _logger.LogInformation("Salarié {SalarieId} retiré de l'entreprise {EntrepriseId}, codes révoqués", _lien.SalarieId, _entreprise.Id);
        return (true, "Salarié retiré et ses codes liés révoqués.");
    }

    private static DTO_EntrepriseAffichage VersDTO(E_Entreprise p_entreprise)
    {
        return new DTO_EntrepriseAffichage
        {
            Id = p_entreprise.Id,
            Nom = p_entreprise.Nom,
            Adresse = p_entreprise.Adresse,
            Siret = p_entreprise.Siret,
            DateCreation = p_entreprise.DateCreation,
            EstAutorisee = p_entreprise.EstAutorisee
        };
    }
}
