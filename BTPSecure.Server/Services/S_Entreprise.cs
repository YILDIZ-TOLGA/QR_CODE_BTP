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
    private readonly S_Email _sEmail;
    private readonly S_Code _sCode;
    private readonly ILogger<S_Entreprise> _logger;

    public S_Entreprise(DAO_Entreprise p_daoEntreprise, DAO_Utilisateur p_daoUtilisateur,
        DAO_Code p_daoCode, S_Email p_sEmail, S_Code p_sCode, ILogger<S_Entreprise> p_logger)
    {
        _daoEntreprise = p_daoEntreprise;
        _daoUtilisateur = p_daoUtilisateur;
        _daoCode = p_daoCode;
        _sEmail = p_sEmail;
        _sCode = p_sCode;
        _logger = p_logger;
    }

    public async Task<(bool Succes, string Message)> ChangerRole(int p_collaborateurId, Enum_RoleEntreprise p_nouveauRole, int p_dirigeantId)
    {
        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_dirigeantId);
        if (_entreprise == null)
            return (false, "Vous n'avez pas d'entreprise.");

        var _lien = await _daoEntreprise.ObtenirLienCollaborateur(p_collaborateurId, _entreprise.Id);
        if (_lien == null)
            return (false, "Ce collaborateur n'est pas dans votre entreprise.");

        if (_lien.StatutInvitation != Enum_StatutInvitation.Acceptee)
            return (false, "Ce collaborateur n'a pas encore accepté l'invitation.");

        _lien.RoleEntreprise = p_nouveauRole;
        await _daoEntreprise.Sauvegarder();

        if (p_nouveauRole == Enum_RoleEntreprise.Responsable || p_nouveauRole == Enum_RoleEntreprise.ResponsableAdmin)
        {
            await _sCode.CreerCodePermanent(p_collaborateurId, _entreprise);
        }
        else
        {
            await _sCode.RevoquerCodePermanent(p_collaborateurId, _entreprise.Id);
        }

        _logger.LogInformation("Rôle du collaborateur {CollaborateurId} changé en {Role} par dirigeant {DirigeantId}", p_collaborateurId, p_nouveauRole, p_dirigeantId);
        return (true, "Rôle mis à jour.");
    }

    public async Task<(bool Succes, string Message)> CreerCollaborateur(DTO_CreerCollaborateur p_dto, int p_callerId)
    {
        if (string.IsNullOrWhiteSpace(p_dto.Email))
            return (false, "L'email est obligatoire.");
        if (string.IsNullOrWhiteSpace(p_dto.Nom) || string.IsNullOrWhiteSpace(p_dto.Prenom))
            return (false, "Le nom et le prénom sont obligatoires.");

        // Autorisé : le Dirigeant de l'entreprise OU un Responsable Admin de celle-ci
        var _estDirigeant = false;
        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_callerId);
        if (_entreprise != null)
        {
            _estDirigeant = true;
        }
        else
        {
            var _lienRa = await _daoEntreprise.ObtenirPremierLienResponsableAdmin(p_callerId);
            if (_lienRa != null)
                _entreprise = _lienRa.Entreprise;
        }
        if (_entreprise == null)
            return (false, "Vous n'êtes pas autorisé à créer un collaborateur.");

        // Anti-escalade : un Responsable Admin ne peut pas créer un autre Responsable Admin
        if (!_estDirigeant && p_dto.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin)
            return (false, "Seul le dirigeant peut créer un Responsable Admin.");

        if (await _daoUtilisateur.EmailExiste(p_dto.Email))
            return (false, "Un compte avec cet email existe déjà.");

        var _motDePasseTemporaire = GenererMotDePasseTemporaire();
        var _sel = BCrypt.Net.BCrypt.GenerateSalt();
        var _hash = BCrypt.Net.BCrypt.HashPassword(_motDePasseTemporaire, _sel);

        var _collaborateur = new E_Utilisateur
        {
            Email = p_dto.Email.Trim().ToLower(),
            MotDePasseHash = _hash,
            Sel = _sel,
            Nom = p_dto.Nom.Trim(),
            Prenom = p_dto.Prenom.Trim(),
            Telephone = p_dto.Telephone?.Trim(),
            Role = Enum_Role.Collaborateur,
            EstActif = true,
            EmailVerifie = true
        };

        await _daoUtilisateur.Creer(_collaborateur);

        var _lien = new E_CollaborateurEntreprise
        {
            CollaborateurId = _collaborateur.Id,
            EntrepriseId = _entreprise.Id,
            StatutInvitation = Enum_StatutInvitation.Acceptee
        };
        await _daoEntreprise.AjouterCollaborateur(_lien);
        _lien.StatutInvitation = Enum_StatutInvitation.Acceptee;
        _lien.RoleEntreprise = p_dto.RoleEntreprise;
        await _daoEntreprise.Sauvegarder();

        // Code permanent si créé directement en Responsable / Responsable Admin
        if (p_dto.RoleEntreprise == Enum_RoleEntreprise.Responsable || p_dto.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin)
        {
            await _sCode.CreerCodePermanent(_collaborateur.Id, _entreprise);
        }

        _logger.LogInformation("Collaborateur {Email} créé par {CallerId} pour entreprise {EntrepriseId}",
            _collaborateur.Email, p_callerId, _entreprise.Id);

        var _emailCopie = _collaborateur.Email;
        var _prenomCopie = _collaborateur.Prenom;
        var _mdpCopie = _motDePasseTemporaire;
        var _nomEntrepriseCopie = _entreprise.Nom;
        _ = Task.Run(async () =>
        {
            await _sEmail.EnvoyerCompteCreeParDirigeant(_emailCopie, _prenomCopie, _mdpCopie, _nomEntrepriseCopie);
        });

        return (true, "Collaborateur créé. Il recevra ses identifiants par email.");
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

    public async Task<(bool Succes, string Message, DTO_EntrepriseAffichage? Entreprise)> Creer(DTO_CreerEntreprise p_dto, int p_dirigeantId)
    {
        if (string.IsNullOrWhiteSpace(p_dto.Nom))
            return (false, "Le nom de l'entreprise est obligatoire.", null);

        var _existante = await _daoEntreprise.ObtenirParDirigeantId(p_dirigeantId);
        if (_existante != null)
            return (false, "Vous avez déjà une entreprise.", null);

        var _entreprise = new E_Entreprise
        {
            Nom = p_dto.Nom.Trim(),
            Adresse = p_dto.Adresse?.Trim(),
            Siret = p_dto.Siret?.Trim(),
            DirigeantId = p_dirigeantId
        };

        await _daoEntreprise.Creer(_entreprise);
        _logger.LogInformation("Entreprise '{Nom}' créée par dirigeant {DirigeantId}", _entreprise.Nom, p_dirigeantId);

        return (true, "Entreprise créée avec succès.", VersDTO(_entreprise));
    }

    public async Task<DTO_EntrepriseAffichage?> ObtenirParDirigeant(int p_dirigeantId)
    {
        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_dirigeantId);
        return _entreprise == null ? null : VersDTO(_entreprise);
    }

    public async Task<(bool Succes, string Message, DTO_CollaborateurAffichage? Collaborateur)> AjouterCollaborateur(string p_email, int p_dirigeantId)
    {
        if (string.IsNullOrWhiteSpace(p_email))
            return (false, "L'email est obligatoire.", null);

        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_dirigeantId);
        if (_entreprise == null)
            return (false, "Vous n'avez pas encore d'entreprise.", null);

        var _collaborateur = await _daoUtilisateur.ObtenirParEmail(p_email);
        if (_collaborateur == null)
            return (false, "Aucun utilisateur trouvé avec cet email.", null);

        if (_collaborateur.Role != Enum_Role.Collaborateur)
            return (false, "Cet utilisateur n'a pas le rôle Collaborateur.", null);

        if (await _daoEntreprise.CollaborateurEstDansEntreprise(_collaborateur.Id, _entreprise.Id))
            return (false, "Ce collaborateur est déjà dans votre entreprise.", null);

        if (await _daoEntreprise.InvitationExiste(_collaborateur.Id, _entreprise.Id))
            return (false, "Une invitation est déjà en attente pour ce collaborateur.", null);

        var _lien = new E_CollaborateurEntreprise
        {
            CollaborateurId = _collaborateur.Id,
            EntrepriseId = _entreprise.Id
        };

        await _daoEntreprise.AjouterCollaborateur(_lien);
        _logger.LogInformation("Salarié {Email} ajouté à l'entreprise {EntrepriseId}", p_email, _entreprise.Id);

        return (true, "Invitation envoyée avec succès.", new DTO_CollaborateurAffichage
        {
            Id = _lien.Id,
            CollaborateurId = _collaborateur.Id,
            Nom = _collaborateur.Nom,
            Prenom = _collaborateur.Prenom,
            Email = _collaborateur.Email,
            DateAjout = _lien.DateAjout,
            StatutInvitation = _lien.StatutInvitation
        });
    }

    public async Task<List<DTO_CollaborateurAffichage>> ObtenirCollaborateurs(int p_dirigeantId)
    {
        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_dirigeantId);
        if (_entreprise == null) return new List<DTO_CollaborateurAffichage>();

        var _liens = await _daoEntreprise.ObtenirCollaborateurs(_entreprise.Id);
        return _liens.Select(l => new DTO_CollaborateurAffichage
        {
            Id = l.Id,
            CollaborateurId = l.CollaborateurId,
            Nom = l.Collaborateur.Nom,
            Prenom = l.Collaborateur.Prenom,
            Email = l.Collaborateur.Email,
            DateAjout = l.DateAjout,
            StatutInvitation = l.StatutInvitation,
            RoleEntreprise = l.RoleEntreprise
        }).ToList();
    }

    public async Task<(bool Succes, string Message)> RetirerCollaborateur(int p_lienId, int p_dirigeantId)
    {
        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_dirigeantId);
        if (_entreprise == null)
            return (false, "Vous n'avez pas d'entreprise.");

        var _liens = await _daoEntreprise.ObtenirCollaborateurs(_entreprise.Id);
        var _lien = _liens.FirstOrDefault(l => l.Id == p_lienId);
        if (_lien == null)
            return (false, "Ce collaborateur n'est pas dans votre entreprise.");

        await _daoEntreprise.RetirerCollaborateur(_lien);
        await _daoCode.RevoquerCodesParCollaborateurEtEntreprise(_lien.CollaborateurId, _entreprise.Id);

        _logger.LogInformation("Salarié {CollaborateurId} retiré de l'entreprise {EntrepriseId}, codes révoqués", _lien.CollaborateurId, _entreprise.Id);
        return (true, "Collaborateur retiré et ses codes liés révoqués.");
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
