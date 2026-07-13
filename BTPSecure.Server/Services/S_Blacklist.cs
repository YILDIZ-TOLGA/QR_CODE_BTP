using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;

namespace BTPSecure.Server.Services;

public class S_Blacklist
{
    private readonly DAO_Blacklist _dao;
    private readonly DAO_Utilisateur _daoUtilisateur;
    private readonly ILogger<S_Blacklist> _logger;

    public S_Blacklist(DAO_Blacklist p_dao, DAO_Utilisateur p_daoUtilisateur, ILogger<S_Blacklist> p_logger)
    {
        _dao = p_dao;
        _daoUtilisateur = p_daoUtilisateur;
        _logger = p_logger;
    }

    // La blacklist appartient au compte fournisseur principal (racine)
    private async Task<int> ObtenirRacineId(int p_userId)
    {
        var _u = await _daoUtilisateur.ObtenirParId(p_userId);
        if (_u == null)
            return p_userId;
        if (_u.ParentFournisseurId.HasValue)
            return _u.ParentFournisseurId.Value;
        return p_userId;
    }

    public async Task<List<DTO_Blacklist>> Lister(int p_userId)
    {
        var _racineId = await ObtenirRacineId(p_userId);
        var _liste = await _dao.Lister(_racineId);
        return _liste.Select(b => new DTO_Blacklist
        {
            Id = b.Id,
            Email = b.Email,
            DateCreation = b.DateCreation
        }).ToList();
    }

    public async Task<(bool Succes, string Message)> Ajouter(string p_email, int p_userId)
    {
        if (string.IsNullOrWhiteSpace(p_email))
            return (false, "L'email est obligatoire.");

        var _email = p_email.Trim().ToLower();
        var _racineId = await ObtenirRacineId(p_userId);

        if (await _dao.Existe(_racineId, _email))
            return (false, "Cet email est déjà dans la blacklist.");

        await _dao.Ajouter(new E_Blacklist { FournisseurId = _racineId, Email = _email });
        _logger.LogInformation("Email {Email} blacklisté par fournisseur {RacineId}", _email, _racineId);
        return (true, "Email ajouté à la blacklist.");
    }

    public async Task<(bool Succes, string Message)> Supprimer(int p_id, int p_userId)
    {
        var _b = await _dao.ObtenirParId(p_id);
        var _racineId = await ObtenirRacineId(p_userId);
        if (_b == null || _b.FournisseurId != _racineId)
            return (false, "Entrée non trouvée.");

        await _dao.Supprimer(_b);
        _logger.LogInformation("Email {Email} retiré de la blacklist par {RacineId}", _b.Email, _racineId);
        return (true, "Email retiré de la blacklist.");
    }
}
