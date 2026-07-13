using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using Microsoft.IdentityModel.Tokens;

namespace BTPSecure.Server.Services;

public class S_Auth
{
    private readonly DAO_Utilisateur _daoUtilisateur;
    private readonly IConfiguration _config;
    private readonly ILogger<S_Auth> _logger;
    private readonly BTPSecure.Server.Data.AppDbContext _context;
    private readonly S_Email _sEmail;

    public S_Auth(DAO_Utilisateur p_daoUtilisateur, IConfiguration p_config, ILogger<S_Auth> p_logger,
        BTPSecure.Server.Data.AppDbContext p_context, S_Email p_sEmail)
    {
        _daoUtilisateur = p_daoUtilisateur;
        _config = p_config;
        _logger = p_logger;
        _context = p_context;
        _sEmail = p_sEmail;
    }

    public async Task<(bool Succes, string Message)> DemanderResetMotDePasse(string p_email)
    {
        if (string.IsNullOrWhiteSpace(p_email))
            return (true, "Si un compte existe avec cet email, un lien de réinitialisation vous a été envoyé.");

        var _utilisateur = await _daoUtilisateur.ObtenirParEmail(p_email);
        if (_utilisateur == null || !_utilisateur.EstActif)
        {
            return (true, "Si un compte existe avec cet email, un lien de réinitialisation vous a été envoyé.");
        }

        // Invalider les anciens tokens non utilisés
        var _anciens = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            _context.ResetsMotDePasse.Where(r => r.UtilisateurId == _utilisateur.Id && !r.EstUtilise));
        foreach (var _ancien in _anciens)
        {
            _ancien.EstUtilise = true;
        }

        var _token = GenererToken();
        var _reset = new E_ResetMotDePasse
        {
            UtilisateurId = _utilisateur.Id,
            Token = _token,
            DateCreation = DateTime.UtcNow,
            DateExpiration = DateTime.UtcNow.AddHours(1),
            EstUtilise = false
        };
        _context.ResetsMotDePasse.Add(_reset);
        await _context.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            await _sEmail.EnvoyerResetMotDePasse(_utilisateur.Email, _utilisateur.Prenom, _token);
        });

        _logger.LogInformation("Demande de reset mot de passe pour {Email}", _utilisateur.Email);
        return (true, "Si un compte existe avec cet email, un lien de réinitialisation vous a été envoyé.");
    }

    public async Task<(bool Succes, string Message)> ReinitialiserMotDePasse(string p_token, string p_nouveauMotDePasse)
    {
        if (string.IsNullOrWhiteSpace(p_token))
            return (false, "Lien invalide.");

        if (string.IsNullOrWhiteSpace(p_nouveauMotDePasse) || p_nouveauMotDePasse.Length < 8)
            return (false, "Le mot de passe doit faire minimum 8 caractères.");

        var _reset = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            _context.ResetsMotDePasse, r => r.Token == p_token);

        if (_reset == null)
            return (false, "Lien invalide ou expiré.");

        if (_reset.EstUtilise)
            return (false, "Ce lien a déjà été utilisé.");

        if (_reset.DateExpiration < DateTime.UtcNow)
            return (false, "Ce lien a expiré. Refaites une demande.");

        var _utilisateur = await _daoUtilisateur.ObtenirParId(_reset.UtilisateurId);
        if (_utilisateur == null || !_utilisateur.EstActif)
            return (false, "Compte introuvable.");

        var _sel = BCrypt.Net.BCrypt.GenerateSalt();
        _utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(p_nouveauMotDePasse, _sel);
        _utilisateur.Sel = _sel;

        _reset.EstUtilise = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Mot de passe réinitialisé pour {Email}", _utilisateur.Email);
        return (true, "Mot de passe modifié avec succès. Vous pouvez vous connecter.");
    }

    public async Task<(bool Succes, string Message)> ChangerMotDePasse(int p_utilisateurId, string p_ancien, string p_nouveau)
    {
        if (string.IsNullOrWhiteSpace(p_ancien) || string.IsNullOrWhiteSpace(p_nouveau))
            return (false, "L'ancien et le nouveau mot de passe sont obligatoires.");

        if (p_nouveau.Length < 8)
            return (false, "Le nouveau mot de passe doit faire minimum 8 caractères.");

        var _utilisateur = await _daoUtilisateur.ObtenirParId(p_utilisateurId);
        if (_utilisateur == null || !_utilisateur.EstActif)
            return (false, "Compte introuvable.");

        if (!BCrypt.Net.BCrypt.Verify(p_ancien, _utilisateur.MotDePasseHash))
            return (false, "L'ancien mot de passe est incorrect.");

        if (p_ancien == p_nouveau)
            return (false, "Le nouveau mot de passe doit être différent de l'ancien.");

        var _sel = BCrypt.Net.BCrypt.GenerateSalt();
        _utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(p_nouveau, _sel);
        _utilisateur.Sel = _sel;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Mot de passe changé pour {Email}", _utilisateur.Email);
        return (true, "Mot de passe modifié avec succès.");
    }

    private static string GenererToken()
    {
        var _bytes = new byte[48];
        System.Security.Cryptography.RandomNumberGenerator.Fill(_bytes);
        return Convert.ToBase64String(_bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public async Task<(bool Succes, string Message, DTO_ReponseAuth? Reponse)> Inscrire(DTO_Inscription p_dto)
    {
        if (string.IsNullOrWhiteSpace(p_dto.Email) || string.IsNullOrWhiteSpace(p_dto.MotDePasse))
            return (false, "L'email et le mot de passe sont obligatoires.", null);

        if (p_dto.MotDePasse.Length < 8)
            return (false, "Le mot de passe doit faire minimum 8 caractères.", null);

        if (string.IsNullOrWhiteSpace(p_dto.Nom) || string.IsNullOrWhiteSpace(p_dto.Prenom))
            return (false, "Le nom et le prénom sont obligatoires.", null);

        if (await _daoUtilisateur.EmailExiste(p_dto.Email))
            return (false, "Un compte avec cet email existe déjà.", null);

        string? _siret = null;
        string? _siren = null;
        if (p_dto.Role == BTPSecure.Shared.Enums.Enum_Role.Fournisseur)
        {
            if (string.IsNullOrWhiteSpace(p_dto.Siret))
                return (false, "Le numéro SIRET est obligatoire pour un fournisseur.", null);

            _siret = new string(p_dto.Siret.Where(char.IsDigit).ToArray());
            if (_siret.Length != 14)
                return (false, "Le numéro SIRET doit contenir 14 chiffres.", null);

            if (!string.IsNullOrWhiteSpace(p_dto.Siren))
            {
                _siren = new string(p_dto.Siren.Where(char.IsDigit).ToArray());
                if (_siren.Length != 9)
                    return (false, "Le numéro SIREN doit contenir 9 chiffres.", null);
            }
        }

        var _sel = BCrypt.Net.BCrypt.GenerateSalt();
        var _hash = BCrypt.Net.BCrypt.HashPassword(p_dto.MotDePasse, _sel);

        // Les fournisseurs doivent être validés par un administrateur avant de pouvoir se connecter
        bool _estValide = true;
        if (p_dto.Role == BTPSecure.Shared.Enums.Enum_Role.Fournisseur)
        {
            _estValide = false;
        }

        var _utilisateur = new E_Utilisateur
        {
            Email = p_dto.Email.ToLower(),
            MotDePasseHash = _hash,
            Sel = _sel,
            Nom = p_dto.Nom.Trim(),
            Prenom = p_dto.Prenom.Trim(),
            Telephone = p_dto.Telephone?.Trim(),
            Siret = _siret,
            Siren = _siren,
            Role = p_dto.Role,
            EstActif = true,
            EstValide = _estValide
        };

        _utilisateur.EmailVerifie = false;
        _utilisateur.TokenVerification = GenererToken();
        _utilisateur.TokenVerificationExpiration = DateTime.UtcNow.AddHours(24);

        await _daoUtilisateur.Creer(_utilisateur);
        _logger.LogInformation("Nouvel utilisateur inscrit : {Email} avec le rôle {Role}", _utilisateur.Email, _utilisateur.Role);

        var _tokenCopie = _utilisateur.TokenVerification;
        var _emailCopie = _utilisateur.Email;
        var _prenomCopie = _utilisateur.Prenom;
        _ = Task.Run(async () =>
        {
            await _sEmail.EnvoyerVerificationEmail(_emailCopie, _prenomCopie, _tokenCopie);
        });

        return (true, "Inscription réussie. Vérifiez votre email pour activer votre compte.", null);
    }

    public async Task<(bool Succes, string Message)> VerifierEmail(string p_token)
    {
        if (string.IsNullOrWhiteSpace(p_token))
            return (false, "Lien invalide.");

        var _utilisateur = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            _context.Utilisateurs, u => u.TokenVerification == p_token);

        if (_utilisateur == null)
            return (false, "Lien invalide ou expiré.");

        if (_utilisateur.EmailVerifie)
            return (true, "Votre email est déjà vérifié. Vous pouvez vous connecter.");

        if (!_utilisateur.TokenVerificationExpiration.HasValue || _utilisateur.TokenVerificationExpiration.Value < DateTime.UtcNow)
            return (false, "Ce lien a expiré. Demandez un nouveau lien depuis la page de connexion.");

        _utilisateur.EmailVerifie = true;
        _utilisateur.TokenVerification = null;
        _utilisateur.TokenVerificationExpiration = null;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Email vérifié pour {Email}", _utilisateur.Email);
        return (true, "Email vérifié avec succès. Vous pouvez maintenant vous connecter.");
    }

    public async Task<(bool Succes, string Message)> RenvoyerEmailVerification(string p_email)
    {
        if (string.IsNullOrWhiteSpace(p_email))
            return (true, "Si un compte non vérifié existe avec cet email, un nouveau lien a été envoyé.");

        var _utilisateur = await _daoUtilisateur.ObtenirParEmail(p_email);
        if (_utilisateur == null || !_utilisateur.EstActif || _utilisateur.EmailVerifie)
        {
            return (true, "Si un compte non vérifié existe avec cet email, un nouveau lien a été envoyé.");
        }

        _utilisateur.TokenVerification = GenererToken();
        _utilisateur.TokenVerificationExpiration = DateTime.UtcNow.AddHours(24);
        await _context.SaveChangesAsync();

        var _tokenCopie = _utilisateur.TokenVerification;
        var _emailCopie = _utilisateur.Email;
        var _prenomCopie = _utilisateur.Prenom;
        _ = Task.Run(async () =>
        {
            await _sEmail.EnvoyerVerificationEmail(_emailCopie, _prenomCopie, _tokenCopie);
        });

        return (true, "Si un compte non vérifié existe avec cet email, un nouveau lien a été envoyé.");
    }

    public async Task<(bool Succes, string Message, DTO_ReponseAuth? Reponse)> Connecter(DTO_Connexion p_dto)
    {
        if (string.IsNullOrWhiteSpace(p_dto.Email) || string.IsNullOrWhiteSpace(p_dto.MotDePasse))
            return (false, "L'email et le mot de passe sont obligatoires.", null);

        var _utilisateur = await _daoUtilisateur.ObtenirParEmail(p_dto.Email);
        if (_utilisateur == null || !_utilisateur.EstActif)
            return (false, "Email ou mot de passe incorrect.", null);

        if (!BCrypt.Net.BCrypt.Verify(p_dto.MotDePasse, _utilisateur.MotDePasseHash))
            return (false, "Email ou mot de passe incorrect.", null);

        if (!_utilisateur.EmailVerifie)
            return (false, "Votre email n'est pas vérifié. Consultez votre boîte mail ou demandez un nouveau lien.", null);

        if (_utilisateur.Role == BTPSecure.Shared.Enums.Enum_Role.Fournisseur && !_utilisateur.EstValide)
            return (false, "Votre compte fournisseur est en attente de validation par un administrateur.", null);

        var _token = GenererToken(_utilisateur);

        var _reponse = new DTO_ReponseAuth
        {
            Token = _token,
            Nom = _utilisateur.Nom,
            Prenom = _utilisateur.Prenom,
            Role = _utilisateur.Role,
            UtilisateurId = _utilisateur.Id
        };

        _logger.LogInformation("Connexion réussie pour : {Email}", _utilisateur.Email);
        return (true, "Connexion réussie.", _reponse);
    }

    private string GenererToken(E_Utilisateur p_utilisateur)
    {
        var _cle = Encoding.UTF8.GetBytes(_config["Jwt:Cle"]!);
        var _dureeHeures = int.Parse(_config["Jwt:DureeHeures"]!);

        var _claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, p_utilisateur.Id.ToString()),
            new(ClaimTypes.Email, p_utilisateur.Email),
            new(ClaimTypes.Role, p_utilisateur.Role.ToString())
        };

        var _credentials = new SigningCredentials(
            new SymmetricSecurityKey(_cle),
            SecurityAlgorithms.HmacSha256);

        var _token = new JwtSecurityToken(
            issuer: _config["Jwt:Emetteur"],
            audience: _config["Jwt:Audience"],
            claims: _claims,
            expires: DateTime.UtcNow.AddHours(_dureeHeures),
            signingCredentials: _credentials);

        return new JwtSecurityTokenHandler().WriteToken(_token);
    }
}
