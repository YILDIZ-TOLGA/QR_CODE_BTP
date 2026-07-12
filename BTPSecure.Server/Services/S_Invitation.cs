using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

public class S_Invitation
{
    private readonly DAO_Entreprise _daoEntreprise;
    private readonly DAO_Code _daoCode;
    private readonly ILogger<S_Invitation> _logger;

    public S_Invitation(DAO_Entreprise p_daoEntreprise, DAO_Code p_daoCode, ILogger<S_Invitation> p_logger)
    {
        _daoEntreprise = p_daoEntreprise;
        _daoCode = p_daoCode;
        _logger = p_logger;
    }

    public async Task<List<DTO_InvitationAffichage>> ObtenirInvitations(int p_collaborateurId)
    {
        var _liens = await _daoEntreprise.ObtenirInvitationsParCollaborateur(p_collaborateurId);
        return _liens.Select(l => new DTO_InvitationAffichage
        {
            Id = l.Id,
            NomEntreprise = l.Entreprise.Nom,
            SiretEntreprise = l.Entreprise.Siret,
            NomDirigeant = l.Entreprise.Dirigeant.Nom,
            PrenomDirigeant = l.Entreprise.Dirigeant.Prenom,
            DateAjout = l.DateAjout,
            Statut = l.StatutInvitation
        }).ToList();
    }

    public async Task<(bool Succes, string Message)> Accepter(int p_lienId, int p_collaborateurId)
    {
        var _lien = await _daoEntreprise.ObtenirLienParId(p_lienId);
        if (_lien == null || _lien.CollaborateurId != p_collaborateurId)
            return (false, "Invitation non trouvée.");

        if (_lien.StatutInvitation != Enum_StatutInvitation.EnAttente)
            return (false, "Cette invitation n'est plus en attente.");

        _lien.StatutInvitation = Enum_StatutInvitation.Acceptee;
        await _daoEntreprise.Sauvegarder();

        _logger.LogInformation("Salarié {CollaborateurId} a accepté l'invitation pour l'entreprise {Entreprise}",
            p_collaborateurId, _lien.Entreprise.Nom);
        return (true, $"Vous avez rejoint l'entreprise {_lien.Entreprise.Nom}.");
    }

    public async Task<(bool Succes, string Message)> Refuser(int p_lienId, int p_collaborateurId)
    {
        var _lien = await _daoEntreprise.ObtenirLienParId(p_lienId);
        if (_lien == null || _lien.CollaborateurId != p_collaborateurId)
            return (false, "Invitation non trouvée.");

        if (_lien.StatutInvitation != Enum_StatutInvitation.EnAttente)
            return (false, "Cette invitation n'est plus en attente.");

        _lien.StatutInvitation = Enum_StatutInvitation.Refusee;
        _lien.EstActif = false;
        await _daoEntreprise.Sauvegarder();

        _logger.LogInformation("Salarié {CollaborateurId} a refusé l'invitation pour l'entreprise {Entreprise}",
            p_collaborateurId, _lien.Entreprise.Nom);
        return (true, $"Invitation de {_lien.Entreprise.Nom} refusée.");
    }

    public async Task<(bool Succes, string Message)> Quitter(int p_lienId, int p_collaborateurId)
    {
        var _lien = await _daoEntreprise.ObtenirLienParId(p_lienId);
        if (_lien == null || _lien.CollaborateurId != p_collaborateurId)
            return (false, "Lien non trouvé.");

        if (!_lien.EstActif || _lien.StatutInvitation != Enum_StatutInvitation.Acceptee)
            return (false, "Vous n'êtes pas membre de cette entreprise.");

        _lien.EstActif = false;
        await _daoEntreprise.Sauvegarder();

        // Révoquer les codes actifs du salarié dans cette entreprise
        await _daoCode.RevoquerCodesParCollaborateurEtEntreprise(p_collaborateurId, _lien.EntrepriseId);

        _logger.LogInformation("Salarié {CollaborateurId} a quitté l'entreprise {Entreprise}, codes révoqués",
            p_collaborateurId, _lien.Entreprise.Nom);
        return (true, $"Vous avez quitté l'entreprise {_lien.Entreprise.Nom}.");
    }
}
