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

    public C_Admin(S_Admin p_sAdmin)
    {
        _sAdmin = p_sAdmin;
    }

    [HttpGet("entreprises")]
    public async Task<IActionResult> ObtenirEntreprises()
    {
        var _entreprises = await _sAdmin.ObtenirToutesLesEntreprises();
        return Ok(_entreprises);
    }

    [HttpPost("basculer-autorisation/{p_id}")]
    public async Task<IActionResult> BasculerAutorisation(int p_id)
    {
        var (_succes, _message) = await _sAdmin.BasculerAutorisation(p_id);
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

    [HttpPost("limite-souscomptes/{p_id}")]
    public async Task<IActionResult> ChangerLimiteSousComptes(int p_id, [FromBody] BTPSecure.Shared.DTOs.DTO_LimiteSousComptes p_dto)
    {
        var (_succes, _message) = await _sAdmin.ChangerLimiteSousComptes(p_id, p_dto.Limite);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
