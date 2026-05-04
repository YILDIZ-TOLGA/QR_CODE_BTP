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

    public S_Auth(DAO_Utilisateur p_daoUtilisateur, IConfiguration p_config, ILogger<S_Auth> p_logger)
    {
        _daoUtilisateur = p_daoUtilisateur;
        _config = p_config;
        _logger = p_logger;
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
            EstActif = true
        };

        await _daoUtilisateur.Creer(_utilisateur);
        _logger.LogInformation("Nouvel utilisateur inscrit : {Email} avec le rôle {Role}", _utilisateur.Email, _utilisateur.Role);

        return (true, "Inscription réussie.", null);
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
