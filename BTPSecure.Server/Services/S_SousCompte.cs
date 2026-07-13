using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

public class S_SousCompte
{
    private readonly DAO_Utilisateur _daoUtilisateur;
    private readonly DAO_Code _daoCode;
    private readonly S_Email _sEmail;
    private readonly ILogger<S_SousCompte> _logger;

    public S_SousCompte(DAO_Utilisateur p_daoUtilisateur, DAO_Code p_daoCode, S_Email p_sEmail, ILogger<S_SousCompte> p_logger)
    {
        _daoUtilisateur = p_daoUtilisateur;
        _daoCode = p_daoCode;
        _sEmail = p_sEmail;
        _logger = p_logger;
    }

    // Le compte fournisseur connecté est-il un compte principal (pas un sous-compte) ?
    public async Task<bool> EstPrincipal(int p_userId)
    {
        var _u = await _daoUtilisateur.ObtenirParId(p_userId);
        if (_u == null)
            return false;
        if (_u.Role != Enum_Role.Fournisseur)
            return false;
        if (_u.ParentFournisseurId.HasValue)
            return false;
        return true;
    }

    public async Task<List<DTO_SousCompte>> Lister(int p_parentId)
    {
        var _sousComptes = await _daoUtilisateur.ObtenirSousComptes(p_parentId);
        return _sousComptes.Select(VersDTO).ToList();
    }

    public async Task<(bool Succes, string Message)> Creer(DTO_CreerSousCompte p_dto, int p_parentId)
    {
        if (string.IsNullOrWhiteSpace(p_dto.Email))
            return (false, "L'email est obligatoire.");
        if (string.IsNullOrWhiteSpace(p_dto.Nom) || string.IsNullOrWhiteSpace(p_dto.Prenom))
            return (false, "Le nom et le prénom sont obligatoires.");

        var _parent = await _daoUtilisateur.ObtenirParId(p_parentId);
        if (_parent == null || _parent.Role != Enum_Role.Fournisseur || _parent.ParentFournisseurId.HasValue)
            return (false, "Seul un compte fournisseur principal peut créer des sous-comptes.");

        if (await _daoUtilisateur.EmailExiste(p_dto.Email))
            return (false, "Un compte avec cet email existe déjà.");

        var _motDePasseTemporaire = GenererMotDePasseTemporaire();
        var _sel = BCrypt.Net.BCrypt.GenerateSalt();
        var _hash = BCrypt.Net.BCrypt.HashPassword(_motDePasseTemporaire, _sel);

        var _sousCompte = new E_Utilisateur
        {
            Email = p_dto.Email.Trim().ToLower(),
            MotDePasseHash = _hash,
            Sel = _sel,
            Nom = p_dto.Nom.Trim(),
            Prenom = p_dto.Prenom.Trim(),
            Telephone = p_dto.Telephone?.Trim(),
            // Hérite du SIRET/SIREN du compte principal → voit les mêmes commandes
            Siret = _parent.Siret,
            Siren = _parent.Siren,
            Role = Enum_Role.Fournisseur,
            ParentFournisseurId = _parent.Id,
            EstActif = true,
            EstValide = true,
            EmailVerifie = true
        };

        await _daoUtilisateur.Creer(_sousCompte);
        _logger.LogInformation("Sous-compte fournisseur {Email} créé par {ParentId}", _sousCompte.Email, p_parentId);

        var _emailCopie = _sousCompte.Email;
        var _prenomCopie = _sousCompte.Prenom;
        var _mdpCopie = _motDePasseTemporaire;
        var _nomPrincipal = $"{_parent.Prenom} {_parent.Nom}";
        _ = Task.Run(async () =>
        {
            await _sEmail.EnvoyerIdentifiantsSousCompte(_emailCopie, _prenomCopie, _mdpCopie, _nomPrincipal);
        });

        return (true, "Sous-compte créé. Il recevra ses identifiants par email.");
    }

    public async Task<(bool Succes, string Message)> Desactiver(int p_sousCompteId, int p_parentId)
    {
        var _sousCompte = await _daoUtilisateur.ObtenirParId(p_sousCompteId);
        if (_sousCompte == null || _sousCompte.ParentFournisseurId != p_parentId)
            return (false, "Sous-compte non trouvé.");

        _sousCompte.EstActif = false;
        await _daoUtilisateur.Sauvegarder();

        _logger.LogInformation("Sous-compte {Id} désactivé par {ParentId}", p_sousCompteId, p_parentId);
        return (true, "Sous-compte désactivé.");
    }

    public async Task<(bool Succes, string Message)> Reactiver(int p_sousCompteId, int p_parentId)
    {
        var _sousCompte = await _daoUtilisateur.ObtenirParId(p_sousCompteId);
        if (_sousCompte == null || _sousCompte.ParentFournisseurId != p_parentId)
            return (false, "Sous-compte non trouvé.");

        _sousCompte.EstActif = true;
        await _daoUtilisateur.Sauvegarder();

        _logger.LogInformation("Sous-compte {Id} réactivé par {ParentId}", p_sousCompteId, p_parentId);
        return (true, "Sous-compte réactivé.");
    }

    // Historique des validations du compte principal + de tous ses sous-comptes
    public async Task<List<DTO_ValidationHistorique>> ObtenirHistorique(int p_userId)
    {
        var _u = await _daoUtilisateur.ObtenirParId(p_userId);
        if (_u == null)
            return new List<DTO_ValidationHistorique>();

        // Racine = le compte principal (soi-même si principal, sinon le parent)
        int _racineId = p_userId;
        if (_u.ParentFournisseurId.HasValue)
            _racineId = _u.ParentFournisseurId.Value;

        var _ids = await _daoUtilisateur.ObtenirIdsSousComptes(_racineId);
        _ids.Add(_racineId);

        var _codes = await _daoCode.ObtenirValidationsParFournisseurs(_ids);

        return _codes.Select(c => new DTO_ValidationHistorique
        {
            CodeId = c.Id,
            Valeur = c.Valeur,
            NumeroCommande = c.NumeroCommande,
            NomEntreprise = c.NomEntreprise,
            DateValidation = c.DateValidation!.Value,
            ValidateurId = c.FournisseurId!.Value,
            NomValidateur = c.Fournisseur != null ? $"{c.Fournisseur.Prenom} {c.Fournisseur.Nom}" : "",
            AchatsSupplementaires = c.AchatsSupplementaires
        }).ToList();
    }

    private static string GenererMotDePasseTemporaire()
    {
        const string _chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var _bytes = new byte[12];
        System.Security.Cryptography.RandomNumberGenerator.Fill(_bytes);
        var _result = new char[12];
        for (int i = 0; i < 12; i++)
        {
            _result[i] = _chars[_bytes[i] % _chars.Length];
        }
        return new string(_result);
    }

    private static DTO_SousCompte VersDTO(E_Utilisateur p_u)
    {
        return new DTO_SousCompte
        {
            Id = p_u.Id,
            Nom = p_u.Nom,
            Prenom = p_u.Prenom,
            Email = p_u.Email,
            Telephone = p_u.Telephone,
            DateCreation = p_u.DateCreation,
            EstActif = p_u.EstActif
        };
    }
}
