using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;

namespace BTPSecure.Server.Services;

public class S_FournisseurContact
{
    private readonly DAO_FournisseurContact _dao;
    private readonly ILogger<S_FournisseurContact> _logger;

    public S_FournisseurContact(DAO_FournisseurContact p_dao, ILogger<S_FournisseurContact> p_logger)
    {
        _dao = p_dao;
        _logger = p_logger;
    }

    public async Task<List<DTO_FournisseurContact>> Lister(int p_dirigeantId)
    {
        var _liste = await _dao.ObtenirParDirigeant(p_dirigeantId);
        return _liste.Select(VersDTO).ToList();
    }

    public async Task<(bool Succes, string Message, DTO_FournisseurContact? Contact)> Creer(DTO_CreerFournisseurContact p_dto, int p_dirigeantId)
    {
        var (_valide, _msg, _siret, _siren) = ValiderEntrees(p_dto);
        if (!_valide)
            return (false, _msg, null);

        var _existant = await _dao.ObtenirParDirigeantEtSiret(p_dirigeantId, _siret!);
        if (_existant != null)
            return (false, "Un fournisseur avec ce SIRET existe déjà dans votre carnet.", null);

        var _contact = new E_FournisseurContact
        {
            DirigeantId = p_dirigeantId,
            NomEntreprise = p_dto.NomEntreprise.Trim(),
            Email = p_dto.Email.Trim().ToLower(),
            Siret = _siret!,
            Siren = _siren
        };

        await _dao.Creer(_contact);
        _logger.LogInformation("Fournisseur contact {Nom} créé par dirigeant {DirigeantId}", _contact.NomEntreprise, p_dirigeantId);
        return (true, "Fournisseur ajouté au carnet.", VersDTO(_contact));
    }

    public async Task<(bool Succes, string Message)> Modifier(int p_id, DTO_CreerFournisseurContact p_dto, int p_dirigeantId)
    {
        var _contact = await _dao.ObtenirParId(p_id);
        if (_contact == null || _contact.DirigeantId != p_dirigeantId)
            return (false, "Fournisseur non trouvé.");

        var (_valide, _msg, _siret, _siren) = ValiderEntrees(p_dto);
        if (!_valide)
            return (false, _msg);

        if (_contact.Siret != _siret)
        {
            var _autre = await _dao.ObtenirParDirigeantEtSiret(p_dirigeantId, _siret!);
            if (_autre != null && _autre.Id != p_id)
                return (false, "Un autre fournisseur avec ce SIRET existe déjà.");
        }

        _contact.NomEntreprise = p_dto.NomEntreprise.Trim();
        _contact.Email = p_dto.Email.Trim().ToLower();
        _contact.Siret = _siret!;
        _contact.Siren = _siren;

        await _dao.Sauvegarder();
        _logger.LogInformation("Fournisseur contact {Id} modifié par dirigeant {DirigeantId}", p_id, p_dirigeantId);
        return (true, "Fournisseur mis à jour.");
    }

    public async Task<(bool Succes, string Message)> Supprimer(int p_id, int p_dirigeantId)
    {
        var _contact = await _dao.ObtenirParId(p_id);
        if (_contact == null || _contact.DirigeantId != p_dirigeantId)
            return (false, "Fournisseur non trouvé.");

        await _dao.Supprimer(_contact);
        _logger.LogInformation("Fournisseur contact {Id} supprimé par dirigeant {DirigeantId}", p_id, p_dirigeantId);
        return (true, "Fournisseur supprimé du carnet.");
    }

    private static (bool Valide, string Message, string? Siret, string? Siren) ValiderEntrees(DTO_CreerFournisseurContact p_dto)
    {
        if (string.IsNullOrWhiteSpace(p_dto.NomEntreprise))
            return (false, "Le nom de l'entreprise est obligatoire.", null, null);

        if (string.IsNullOrWhiteSpace(p_dto.Email))
            return (false, "L'email est obligatoire.", null, null);

        if (string.IsNullOrWhiteSpace(p_dto.Siret))
            return (false, "Le SIRET est obligatoire.", null, null);

        var _siret = new string(p_dto.Siret.Where(char.IsDigit).ToArray());
        if (_siret.Length != 14)
            return (false, "Le SIRET doit contenir 14 chiffres.", null, null);

        string? _siren = null;
        if (!string.IsNullOrWhiteSpace(p_dto.Siren))
        {
            _siren = new string(p_dto.Siren.Where(char.IsDigit).ToArray());
            if (_siren.Length != 9)
                return (false, "Le SIREN doit contenir 9 chiffres.", null, null);
        }

        return (true, "", _siret, _siren);
    }

    private static DTO_FournisseurContact VersDTO(E_FournisseurContact p_contact)
    {
        return new DTO_FournisseurContact
        {
            Id = p_contact.Id,
            NomEntreprise = p_contact.NomEntreprise,
            Email = p_contact.Email,
            Siret = p_contact.Siret,
            Siren = p_contact.Siren,
            DateCreation = p_contact.DateCreation
        };
    }
}
