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
    private readonly S_Pdf _sPdf;
    private readonly ILogger<S_Code> _logger;

    private const string CARACTERES_AUTORISES = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public S_Code(DAO_Code p_daoCode, DAO_Entreprise p_daoEntreprise, DAO_Utilisateur p_daoUtilisateur,
        S_Pdf p_sPdf, ILogger<S_Code> p_logger)
    {
        _daoCode = p_daoCode;
        _daoEntreprise = p_daoEntreprise;
        _daoUtilisateur = p_daoUtilisateur;
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
            EntrepriseId = p_dto.EntrepriseId
        };

        if (p_dto.TypeCode == Enum_TypeCode.Confiance)
        {
            _code.DateExpiration = DateTime.UtcNow.AddHours(p_dto.DureeValiditeHeures!.Value);
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
            PrenomSalarie = p_code.Salarie?.Prenom ?? ""
        };
    }
}
