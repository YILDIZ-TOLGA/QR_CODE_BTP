using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

// Vue d'audit réservée au DIRIGEANT : qui détient un code permanent, et l'usage
// qui en est fait. Un Responsable Admin n'y a pas accès (règle de confidentialité :
// il ne doit pas surveiller l'activité de ses collègues).
public class S_HistoriqueCode
{
    private readonly DAO_Entreprise _daoEntreprise;
    private readonly DAO_Code _daoCode;
    private readonly DAO_ValidationCode _daoValidationCode;
    private readonly DAO_Utilisateur _daoUtilisateur;

    public S_HistoriqueCode(DAO_Entreprise p_daoEntreprise, DAO_Code p_daoCode,
        DAO_ValidationCode p_daoValidationCode, DAO_Utilisateur p_daoUtilisateur)
    {
        _daoEntreprise = p_daoEntreprise;
        _daoCode = p_daoCode;
        _daoValidationCode = p_daoValidationCode;
        _daoUtilisateur = p_daoUtilisateur;
    }

    public async Task<List<DTO_PorteurCodePermanent>> ObtenirPorteurs(int p_dirigeantId)
    {
        var _resultat = new List<DTO_PorteurCodePermanent>();

        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_dirigeantId);
        if (_entreprise == null)
            return _resultat;

        var _codes = await _daoCode.ObtenirCodesPermanentsActifs(_entreprise.Id);
        if (_codes.Count == 0)
            return _resultat;

        // Une seule lecture de l'historique : on agrège en mémoire plutôt que
        // de faire une requête par porteur.
        var _historique = await _daoValidationCode.ObtenirParEntreprise(_entreprise.Id);

        var _liens = await _daoEntreprise.ObtenirCollaborateurs(_entreprise.Id);

        foreach (var _code in _codes)
        {
            if (!_code.CollaborateurId.HasValue)
                continue;

            int _porteurId = _code.CollaborateurId.Value;

            var _porteur = await _daoUtilisateur.ObtenirParId(_porteurId);
            if (_porteur == null)
                continue;

            var _dto = new DTO_PorteurCodePermanent
            {
                PorteurId = _porteurId,
                Nom = _porteur.Nom,
                Prenom = _porteur.Prenom,
                Email = _porteur.Email,
                DateCreationCode = _code.DateCreation,
                EstDirigeant = _porteurId == _entreprise.DirigeantId
            };

            if (_dto.EstDirigeant)
            {
                _dto.RoleLibelle = "Dirigeant";
            }
            else
            {
                var _lien = _liens.FirstOrDefault(l => l.CollaborateurId == _porteurId);
                if (_lien != null)
                {
                    _dto.RoleLibelle = LibelleRole(_lien.RoleEntreprise);
                }
            }

            var _siennes = _historique.Where(v => v.PorteurId == _porteurId).ToList();
            _dto.NombreUtilisations = _siennes.Count;
            if (_siennes.Count > 0)
            {
                _dto.DerniereUtilisation = _siennes.Max(v => v.DateValidation);
                _dto.TotalAchatsSupplementaires = _siennes.Sum(v => v.AchatsSupplementaires);
            }

            _resultat.Add(_dto);
        }

        // Le dirigeant en tête, puis les plus actifs
        return _resultat
            .OrderByDescending(d => d.EstDirigeant)
            .ThenByDescending(d => d.NombreUtilisations)
            .ThenBy(d => d.Nom)
            .ToList();
    }

    public async Task<List<DTO_UtilisationCode>> ObtenirUtilisations(int p_dirigeantId, int p_porteurId)
    {
        var _resultat = new List<DTO_UtilisationCode>();

        // Bornage à l'entreprise du dirigeant : impossible de lire l'historique d'une autre
        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_dirigeantId);
        if (_entreprise == null)
            return _resultat;

        var _validations = await _daoValidationCode.ObtenirParPorteur(_entreprise.Id, p_porteurId);

        foreach (var _v in _validations)
        {
            var _dto = new DTO_UtilisationCode
            {
                Id = _v.Id,
                DateValidation = _v.DateValidation,
                ValeurUtilisee = _v.ValeurUtilisee,
                NumeroCommande = _v.NumeroCommande,
                AchatsSupplementaires = _v.AchatsSupplementaires,
                EstPermanent = _v.EstPermanent
            };

            if (_v.Validateur != null)
            {
                _dto.NomValidateur = $"{_v.Validateur.Prenom} {_v.Validateur.Nom}".Trim();
                _dto.EmailValidateur = _v.Validateur.Email;
                if (!string.IsNullOrWhiteSpace(_v.Validateur.NomSociete))
                {
                    _dto.SocieteValidateur = _v.Validateur.NomSociete;
                }
            }

            _resultat.Add(_dto);
        }

        return _resultat;
    }

    // H_RoleEntreprise est côté client : équivalent pour les libellés serveur
    private static string LibelleRole(Enum_RoleEntreprise p_role)
    {
        if (p_role == Enum_RoleEntreprise.Responsable)
        {
            return "Responsable";
        }
        if (p_role == Enum_RoleEntreprise.ResponsableAdmin)
        {
            return "Responsable Admin";
        }
        return "Collaborateur";
    }
}
