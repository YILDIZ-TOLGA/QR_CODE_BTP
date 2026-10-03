using System.Security.Claims;
using BTPSecure.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class C_Admin : ControllerBase
{
    private readonly S_Admin _sAdmin;
    private readonly S_Apporteur _sApporteur;

    public C_Admin(S_Admin p_sAdmin, S_Apporteur p_sApporteur)
    {
        _sAdmin = p_sAdmin;
        _sApporteur = p_sApporteur;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // Apporteurs d'affaires : qui payer, et combien
    [HttpGet("apporteurs")]
    public async Task<IActionResult> ObtenirApporteurs()
    {
        var _liste = await _sApporteur.ObtenirApporteursPourAdmin();
        return Ok(_liste);
    }

    [HttpGet("entreprises")]
    public async Task<IActionResult> ObtenirEntreprises()
    {
        var _entreprises = await _sAdmin.ObtenirToutesLesEntreprises();
        return Ok(_entreprises);
    }

    // Le corps porte le montant de la commission — ignoré si l'entreprise n'est pas
    // parrainée ou si son parrainage a déjà été validé (voir S_Admin).
    [HttpPost("basculer-autorisation/{p_id}")]
    public async Task<IActionResult> BasculerAutorisation(int p_id, [FromBody] BTPSecure.Shared.DTOs.DTO_AutoriserEntreprise p_dto)
    {
        var _montant = 0m;
        if (p_dto != null)
        {
            _montant = p_dto.MontantCommission;
        }

        var (_succes, _message) = await _sAdmin.BasculerAutorisation(p_id, _montant, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpGet("fournisseurs")]
    public async Task<IActionResult> ObtenirFournisseurs()
    {
        var _fournisseurs = await _sAdmin.ObtenirFournisseurs();
        return Ok(_fournisseurs);
    }

    [HttpPost("valider-fournisseur/{p_id}")]
    public async Task<IActionResult> ValiderFournisseur(int p_id)
    {
        var (_succes, _message) = await _sAdmin.ValiderFournisseur(p_id);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    // Bloque / débloque un fournisseur (et ses sous-comptes s'il est principal)
    [HttpPost("basculer-blocage-fournisseur/{p_id}")]
    public async Task<IActionResult> BasculerBlocageFournisseur(int p_id)
    {
        var (_succes, _message) = await _sAdmin.BasculerBlocageFournisseur(p_id);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("limite-responsables/{p_id}")]
    public async Task<IActionResult> ChangerLimiteResponsables(int p_id, [FromBody] BTPSecure.Shared.DTOs.DTO_LimiteResponsables p_dto)
    {
        var (_succes, _message) = await _sAdmin.ChangerLimiteResponsables(p_id, p_dto.Limite);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("limite-souscomptes/{p_id}")]
    public async Task<IActionResult> ChangerLimiteSousComptes(int p_id, [FromBody] BTPSecure.Shared.DTOs.DTO_LimiteSousComptes p_dto)
    {
        var (_succes, _message) = await _sAdmin.ChangerLimiteSousComptes(p_id, p_dto.Limite);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
