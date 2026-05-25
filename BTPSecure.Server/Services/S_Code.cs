using System.Security.Cryptography;
using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

public class S_Code
{
    private readonly DAO_Code _daoCode;
    private readonly DAO_Entreprise _daoEntreprise;
    private readonly DAO_Utilisateur _daoUtilisateur;
    private readonly DAO_FournisseurContact _daoFournisseurContact;
    private readonly S_Pdf _sPdf;
    private readonly ILogger<S_Code> _logger;

    private const string CARACTERES_AUTORISES = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public S_Code(DAO_Code p_daoCode, DAO_Entreprise p_daoEntreprise, DAO_Utilisateur p_daoUtilisateur,
        DAO_FournisseurContact p_daoFournisseurContact, S_Pdf p_sPdf, ILogger<S_Code> p_logger)
    {
        _daoCode = p_daoCode;
        _daoEntreprise = p_daoEntreprise;
        _daoUtilisateur = p_daoUtilisateur;
        _daoFournisseurContact = p_daoFournisseurContact;
        _sPdf = p_sPdf;
        _logger = p_logger;
    }

    public async Task<(bool Succes, string Message, DTO_CodeAffichage? Code)> Creer(DTO_CreerCode p_dto, int p_patronId)
    {
        if (string.IsNullOrWhiteSpace(p_dto.NumeroCommande))
            return (false, "Le numéro de commande est obligatoire.", null);

        var _entreprise = await _daoEntreprise.ObtenirParId(p_dto.EntrepriseId);
        if (_entreprise == null || _entreprise.PatronId != p_patronId)
            return (false, "Entreprise non trouvée ou vous n'en êtes pas le patron.", null);

        if (!_entreprise.EstAutorisee)
            return (false, "Votre entreprise n'est pas encore autorisée par l'administrateur à créer des codes.", null);

        if (!await _daoEntreprise.SalarieEstDansEntreprise(p_dto.SalarieId, p_dto.EntrepriseId))
            return (false, "Ce salarié n'appartient pas à votre entreprise.", null);

        var _salarie = await _daoUtilisateur.ObtenirParId(p_dto.SalarieId);
        if (_salarie == null)
            return (false, "Salarié non trouvé.", null);

        if (p_dto.TypeCode == Enum_TypeCode.Confiance && (!p_dto.DureeValiditeHeures.HasValue || p_dto.DureeValiditeHeures.Value <= 0))
            return (false, "La durée de validité est obligatoire pour un code Confiance.", null);

        if (p_dto.TypeCode == Enum_TypeCode.Liste && string.IsNullOrWhiteSpace(p_dto.ListeMateriaux))
            return (false, "La liste des matériaux est obligatoire pour un code Liste.", null);

        int? _fournisseurContactId = null;
        string? _reference = null;
        if (p_dto.UtiliserFournisseur)
        {
            E_FournisseurContact? _contact = null;

            if (p_dto.FournisseurContactId.HasValue)
            {
                _contact = await _daoFournisseurContact.ObtenirParId(p_dto.FournisseurContactId.Value);
                if (_contact == null || _contact.PatronId != p_patronId)
                    return (false, "Fournisseur sélectionné introuvable.", null);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(p_dto.NouveauFournisseurNomEntreprise)
                    || string.IsNullOrWhiteSpace(p_dto.NouveauFournisseurEmail)
                    || string.IsNullOrWhiteSpace(p_dto.NouveauFournisseurSiret))
                    return (false, "Nom, email et SIRET du nouveau fournisseur sont obligatoires.", null);

                var _siret = new string(p_dto.NouveauFournisseurSiret.Where(char.IsDigit).ToArray());
                if (_siret.Length != 14)
                    return (false, "Le SIRET du nouveau fournisseur doit contenir 14 chiffres.", null);

                string? _siren = null;
                if (!string.IsNullOrWhiteSpace(p_dto.NouveauFournisseurSiren))
                {
                    _siren = new string(p_dto.NouveauFournisseurSiren.Where(char.IsDigit).ToArray());
                    if (_siren.Length != 9)
                        return (false, "Le SIREN du nouveau fournisseur doit contenir 9 chiffres.", null);
                }

                _contact = await _daoFournisseurContact.ObtenirParPatronEtSiret(p_patronId, _siret);
                if (_contact == null)
                {
                    _contact = new E_FournisseurContact
                    {
                        PatronId = p_patronId,
                        NomEntreprise = p_dto.NouveauFournisseurNomEntreprise.Trim(),
                        Email = p_dto.NouveauFournisseurEmail.Trim().ToLower(),
                        Siret = _siret,
                        Siren = _siren
                    };
                    await _daoFournisseurContact.Creer(_contact);
                }
            }

            _fournisseurContactId = _contact.Id;
            var _nomNettoye = _entreprise.Nom.Replace(" ", "-");
            _reference = $"{_nomNettoye}-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        }

        var _valeur = await GenererValeurUnique();

        var _code = new E_Code
        {
            Valeur = _valeur,
            TypeCode = p_dto.TypeCode,
            NumeroCommande = p_dto.NumeroCommande.Trim(),
            NomEntreprise = _entreprise.Nom,
            Info = p_dto.Info?.Trim(),
            ListeMateriaux = p_dto.ListeMateriaux?.Trim(),
            DureeValidite = p_dto.DureeValiditeHeures,
            PatronId = p_patronId,
            SalarieId = p_dto.SalarieId,
            EntrepriseId = p_dto.EntrepriseId,
            FournisseurContactId = _fournisseurContactId,
            Reference = _reference
        };

        if (p_dto.TypeCode == Enum_TypeCode.Confiance)
        {
            _code.DateExpiration = DateTime.UtcNow.AddHours(p_dto.DureeValiditeHeures!.Value);
        }
        else if (p_dto.TypeCode == Enum_TypeCode.Liste)
        {
            _code.DateExpiration = DateTime.UtcNow.AddDays(7);
        }

        await _daoCode.Creer(_code);
        _code.Salarie = _salarie;

        _logger.LogInformation("Code {Valeur} créé par patron {PatronId} pour salarié {SalarieId}", _valeur, p_patronId, p_dto.SalarieId);

        return (true, "Code créé avec succès.", VersDTO(_code));
    }

    public async Task<List<DTO_CodeAffichage>> ObtenirParPatron(int p_patronId)
    {
        var _codes = await _daoCode.ObtenirParPatron(p_patronId);
        return _codes.Select(VersDTO).ToList();
    }

    public async Task<List<DTO_CodeAffichage>> ObtenirParSalarie(int p_salarieId)
    {
        var _codes = await _daoCode.ObtenirParSalarie(p_salarieId);
        return _codes.Select(VersDTO).ToList();
    }

    public async Task<(bool Succes, string Message, DTO_ResultatValidation? Resultat)> Valider(string p_valeur, int p_fournisseurId)
    {
        var _code = await _daoCode.ObtenirParValeur(p_valeur.Trim().ToUpper());
        if (_code == null)
            return (false, "Code non trouvé.", new DTO_ResultatValidation { EstValide = false, Message = "Code non trouvé." });

        if (_code.Statut != Enum_StatutCode.Actif)
            return (false, "Ce code n'est plus actif.", new DTO_ResultatValidation { EstValide = false, Message = $"Ce code est {_code.Statut.ToString().ToLower()}." });

        if (_code.DateExpiration.HasValue && _code.DateExpiration.Value < DateTime.UtcNow)
        {
            _code.Statut = Enum_StatutCode.Expire;
            await _daoCode.Sauvegarder();
            return (false, "Ce code est expiré.", new DTO_ResultatValidation { EstValide = false, Message = "Ce code est expiré." });
        }

        _code.FournisseurId = p_fournisseurId;
        _code.DateValidation = DateTime.UtcNow;

        if (_code.TypeCode == Enum_TypeCode.Liste)
        {
            _code.Statut = Enum_StatutCode.Utilise;
            _code.DateExpiration = DateTime.UtcNow.AddMinutes(10);
        }
        else if (_code.TypeCode == Enum_TypeCode.Confiance)
        {
            _code.Valeur = await GenererValeurUnique();
        }

        await _daoCode.Sauvegarder();

        var _pdf = _sPdf.GenererConfirmation(_code);

        var _resultat = new DTO_ResultatValidation
        {
            EstValide = true,
            Message = "Code validé avec succès.",
            NomSalarie = _code.Salarie.Nom,
            PrenomSalarie = _code.Salarie.Prenom,
            NumeroCommande = _code.NumeroCommande,
            NomEntreprise = _code.NomEntreprise,
            ListeMateriaux = _code.ListeMateriaux,
            Info = _code.Info,
            PdfBase64 = Convert.ToBase64String(_pdf)
        };

        _logger.LogInformation("Code {Valeur} validé par fournisseur {FournisseurId}", p_valeur, p_fournisseurId);
        return (true, "Code validé.", _resultat);
    }

    public async Task<(bool Succes, string Message)> Revoquer(int p_codeId, int p_patronId)
    {
        var _code = await _daoCode.ObtenirParId(p_codeId);
        if (_code == null)
            return (false, "Code non trouvé.");

        if (_code.PatronId != p_patronId)
            return (false, "Vous n'êtes pas autorisé à révoquer ce code.");

        if (_code.Statut != Enum_StatutCode.Actif)
            return (false, "Seuls les codes actifs peuvent être révoqués.");

        _code.Statut = Enum_StatutCode.Revoque;
        await _daoCode.Sauvegarder();

        _logger.LogInformation("Code {Id} révoqué par patron {PatronId}", p_codeId, p_patronId);
        return (true, "Code révoqué avec succès.");
    }

    private async Task<string> GenererValeurUnique()
    {
        string _valeur;
        do
        {
            _valeur = GenererCode();
        } while (await _daoCode.ValeurExiste(_valeur));

        return _valeur;
    }

    private static string GenererCode()
    {
        var _chars = new char[8];
        for (int i = 0; i < 8; i++)
        {
            _chars[i] = CARACTERES_AUTORISES[RandomNumberGenerator.GetInt32(CARACTERES_AUTORISES.Length)];
        }
        return $"{new string(_chars, 0, 4)}-{new string(_chars, 4, 4)}";
    }

    public async Task<(bool Succes, string Message, DTO_ResultatValidation? Resultat)> ValiderPourCommande(int p_codeId, string p_valeur, int p_fournisseurId)
    {
        var _code = await _daoCode.ObtenirParId(p_codeId);
        if (_code == null)
            return (false, "Commande introuvable.", new DTO_ResultatValidation { EstValide = false, Message = "Commande introuvable." });

        if (_code.Statut != Enum_StatutCode.Actif)
            return (false, "Cette commande n'est plus active.", new DTO_ResultatValidation { EstValide = false, Message = $"Cette commande est {_code.Statut.ToString().ToLower()}." });

        var _valeurSaisie = p_valeur.Trim().ToUpper();
        if (_code.Valeur != _valeurSaisie)
            return (false, "Le code ne correspond pas à la référence associée.",
                new DTO_ResultatValidation { EstValide = false, Message = "Le code ne correspond pas à la référence associée." });

        return await Valider(_valeurSaisie, p_fournisseurId);
    }

    public async Task<List<DTO_CommandeAVenir>> ObtenirCommandesAVenir(int p_fournisseurId)
    {
        var _utilisateur = await _daoUtilisateur.ObtenirParId(p_fournisseurId);
        if (_utilisateur == null || string.IsNullOrEmpty(_utilisateur.Siret))
            return new List<DTO_CommandeAVenir>();

        var _codes = await _daoCode.ObtenirCommandesPourFournisseur(_utilisateur.Siret, _utilisateur.Siren);

        return _codes.Select(c => new DTO_CommandeAVenir
        {
            CodeId = c.Id,
            Reference = c.Reference ?? "",
            NomEntreprisePatron = c.NomEntreprise,
            NumeroCommande = c.NumeroCommande,
            DateCreation = c.DateCreation,
            DateExpiration = c.DateExpiration,
            ListeMateriaux = c.ListeMateriaux,
            Info = c.Info,
            TypeCode = c.TypeCode,
            EstPrete = c.EstPrete,
            DatePrete = c.DatePrete
        }).ToList();
    }

    public async Task<(bool Succes, string Message)> MarquerPrete(int p_codeId, int p_fournisseurId)
    {
        var _code = await _daoCode.ObtenirParId(p_codeId);
        if (_code == null)
            return (false, "Commande introuvable.");

        if (_code.Statut != Enum_StatutCode.Actif)
            return (false, "Cette commande n'est plus active.");

        var _utilisateur = await _daoUtilisateur.ObtenirParId(p_fournisseurId);
        if (_utilisateur == null || string.IsNullOrEmpty(_utilisateur.Siret))
            return (false, "Fournisseur introuvable.");

        if (_code.FournisseurContactId == null)
            return (false, "Cette commande n'a pas de fournisseur associé.");

        var _contact = await _daoFournisseurContact.ObtenirParId(_code.FournisseurContactId.Value);
        if (_contact == null)
            return (false, "Fournisseur associé introuvable.");

        if (_contact.Siret != _utilisateur.Siret)
            return (false, "Vous n'êtes pas le fournisseur de cette commande.");

        bool _sirenContactPresent = !string.IsNullOrEmpty(_contact.Siren);
        bool _sirenUserPresent = !string.IsNullOrEmpty(_utilisateur.Siren);
        if (_sirenContactPresent != _sirenUserPresent)
            return (false, "Vous n'êtes pas le fournisseur de cette commande.");
        if (_sirenContactPresent && _sirenUserPresent && _contact.Siren != _utilisateur.Siren)
            return (false, "Vous n'êtes pas le fournisseur de cette commande.");

        if (_code.EstPrete)
            return (false, "Cette commande est déjà marquée comme prête.");

        _code.EstPrete = true;
        _code.DatePrete = DateTime.UtcNow;
        await _daoCode.Sauvegarder();

        _logger.LogInformation("Commande {CodeId} marquée prête par fournisseur {FournisseurId}", p_codeId, p_fournisseurId);
        return (true, "Commande marquée comme prête.");
    }

    public async Task<List<DTO_NotificationPatron>> ObtenirNotificationsPatron(int p_patronId)
    {
        var _codes = await _daoCode.ObtenirNotificationsPourPatron(p_patronId);

        return _codes.Select(c => new DTO_NotificationPatron
        {
            CodeId = c.Id,
            Reference = c.Reference ?? "",
            NumeroCommande = c.NumeroCommande,
            NomFournisseur = c.FournisseurContact?.NomEntreprise ?? "",
            EmailFournisseur = c.FournisseurContact?.Email ?? "",
            NomSalarie = c.Salarie?.Nom ?? "",
            PrenomSalarie = c.Salarie?.Prenom ?? "",
            DatePrete = c.DatePrete ?? DateTime.UtcNow,
            DateExpiration = c.DateExpiration,
            ListeMateriaux = c.ListeMateriaux,
            Info = c.Info
        }).ToList();
    }

    private static DTO_CodeAffichage VersDTO(E_Code p_code)
    {
        return new DTO_CodeAffichage
        {
            Id = p_code.Id,
            Valeur = p_code.Valeur,
            TypeCode = p_code.TypeCode,
            Statut = p_code.Statut,
            NumeroCommande = p_code.NumeroCommande,
            NomEntreprise = p_code.NomEntreprise,
            Info = p_code.Info,
            ListeMateriaux = p_code.ListeMateriaux,
            DateCreation = p_code.DateCreation,
            DateExpiration = p_code.DateExpiration,
            NomSalarie = p_code.Salarie?.Nom ?? "",
            PrenomSalarie = p_code.Salarie?.Prenom ?? "",
            Reference = p_code.Reference,
            NomFournisseurContact = p_code.FournisseurContact?.NomEntreprise
        };
    }
}
