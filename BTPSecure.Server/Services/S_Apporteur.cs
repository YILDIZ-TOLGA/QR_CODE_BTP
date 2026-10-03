using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;
using BTPSecure.Shared.Helpers;

namespace BTPSecure.Server.Services;

// Apporteurs d'affaires : attribution du code de parrainage, enregistrement des
// filleuls et restitution du tableau de bord.
public class S_Apporteur
{
    private readonly DAO_Parrainage _daoParrainage;
    private readonly DAO_Entreprise _daoEntreprise;
    private readonly DAO_Utilisateur _daoUtilisateur;
    private readonly ILogger<S_Apporteur> _logger;

    public S_Apporteur(DAO_Parrainage p_daoParrainage, DAO_Entreprise p_daoEntreprise,
        DAO_Utilisateur p_daoUtilisateur, ILogger<S_Apporteur> p_logger)
    {
        _daoParrainage = p_daoParrainage;
        _daoEntreprise = p_daoEntreprise;
        _daoUtilisateur = p_daoUtilisateur;
        _logger = p_logger;
    }

    // Crée le compte d'un apporteur en lui attribuant son code (ex. « TY-01 »).
    // Renvoie le code attribué, ou null si l'attribution a échoué.
    public async Task<string?> CreerApporteur(E_Utilisateur p_utilisateur)
    {
        var _prefixe = H_CodeParrainage.Initiales(p_utilisateur.Nom, p_utilisateur.Prenom);
        var _code = await _daoParrainage.CreerApporteur(p_utilisateur, _prefixe);

        if (_code == null)
        {
            _logger.LogError("Attribution du code de parrainage impossible pour {Email} (prefixe {Prefixe})", p_utilisateur.Email, _prefixe);
            return null;
        }

        _logger.LogInformation("Apporteur d'affaires {Email} cree avec le code {Code}", p_utilisateur.Email, _code);
        return _code;
    }

    // Vérifie qu'un code de parrainage saisi correspond bien à un apporteur actif.
    // Renvoie l'apporteur, ou null si le code est inconnu.
    public async Task<E_Utilisateur?> ResoudreApporteur(string? p_codeSaisi)
    {
        var _code = H_CodeParrainage.Normaliser(p_codeSaisi);
        if (_code.Length == 0)
            return null;

        return await _daoParrainage.ObtenirApporteurParCode(_code);
    }

    // Enregistre le lien de parrainage à l'inscription d'un filleul.
    // Le parrainage naît EN ATTENTE : la commission n'existe qu'une fois l'entreprise
    // autorisée par un admin.
    public async Task EnregistrerParrainage(int p_apporteurId, int p_filleulId, string p_code)
    {
        await _daoParrainage.Creer(new E_Parrainage
        {
            ApporteurId = p_apporteurId,
            FilleulId = p_filleulId,
            CodeUtilise = p_code,
            DateInscription = DateTime.UtcNow,
            Statut = Enum_StatutParrainage.EnAttente,
            MontantCommission = 0m
        });

        _logger.LogInformation("Parrainage enregistre : apporteur {Apporteur} -> filleul {Filleul} (code {Code})", p_apporteurId, p_filleulId, p_code);
    }

    // Tableau de bord de l'apporteur connecté. Borné à son propre identifiant :
    // un apporteur ne voit jamais les filleuls d'un autre.
    public async Task<DTO_TableauApporteur> ObtenirTableauDeBord(int p_apporteurId)
    {
        var _resultat = new DTO_TableauApporteur();

        // Son propre code : c'est la premiere chose qu'il vient chercher
        var _apporteur = await _daoUtilisateur.ObtenirParId(p_apporteurId);
        if (_apporteur != null && _apporteur.CodeParrainage != null)
        {
            _resultat.CodeParrainage = _apporteur.CodeParrainage;
        }

        var _parrainages = await _daoParrainage.ObtenirParApporteur(p_apporteurId);

        foreach (var _p in _parrainages)
        {
            var _estValide = false;
            if (_p.Statut == Enum_StatutParrainage.Valide)
            {
                _estValide = true;
            }

            // Nom de l'entreprise créée par le filleul, s'il en a une
            string? _nomEntreprise = null;
            var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(_p.FilleulId);
            if (_entreprise != null)
            {
                _nomEntreprise = _entreprise.Nom;
            }

            var _nom = string.Empty;
            var _prenom = string.Empty;
            if (_p.Filleul != null)
            {
                _nom = _p.Filleul.Nom;
                _prenom = _p.Filleul.Prenom;
            }

            _resultat.Filleuls.Add(new DTO_FilleulAffichage
            {
                ParrainageId = _p.Id,
                NomFilleul = _nom,
                PrenomFilleul = _prenom,
                NomEntreprise = _nomEntreprise,
                DateInscription = _p.DateInscription,
                EstValide = _estValide,
                MontantCommission = _p.MontantCommission,
                DateValidation = _p.DateValidation
            });

            if (_estValide)
            {
                _resultat.NombreValides++;
                _resultat.TotalCumule += _p.MontantCommission;
            }
            else
            {
                _resultat.NombreEnAttente++;
            }
        }

        _resultat.NombreFilleuls = _resultat.Filleuls.Count;
        return _resultat;
    }

    // Liste des apporteurs pour le panel admin : qui payer, et combien.
    public async Task<List<DTO_ApporteurAdmin>> ObtenirApporteursPourAdmin()
    {
        var _resultat = new List<DTO_ApporteurAdmin>();
        var _apporteurs = await _daoParrainage.ObtenirTousLesApporteurs();

        foreach (var _a in _apporteurs)
        {
            var _stats = await _daoParrainage.ObtenirStatistiques(_a.Id);

            var _code = string.Empty;
            if (_a.CodeParrainage != null)
            {
                _code = _a.CodeParrainage;
            }

            _resultat.Add(new DTO_ApporteurAdmin
            {
                Id = _a.Id,
                Nom = _a.Nom,
                Prenom = _a.Prenom,
                Email = _a.Email,
                CodeParrainage = _code,
                DateCreation = _a.DateCreation,
                EstActif = _a.EstActif,
                NombreFilleuls = _stats.Total,
                NombreEnAttente = _stats.EnAttente,
                TotalCumule = _stats.Cumule
            });
        }

        return _resultat;
    }
}
