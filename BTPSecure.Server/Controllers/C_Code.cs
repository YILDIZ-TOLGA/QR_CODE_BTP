using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/codes")]
[Authorize]
public class C_Code : ControllerBase
{
    private readonly S_Code _sCode;

    public C_Code(S_Code p_sCode)
    {
        _sCode = p_sCode;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpPost("creer")]
    [Authorize(Roles = "Patron")]
    public async Task<IActionResult> Creer([FromBody] DTO_CreerCode p_dto)
    {
        var (_succes, _message, _code) = await _sCode.Creer(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_code);
    }

    [HttpGet("patron")]
    [Authorize(Roles = "Patron")]
    public async Task<IActionResult> ObtenirParPatron()
    {
        var _codes = await _sCode.ObtenirParPatron(ObtenirUtilisateurId());
        return Ok(_codes);
    }

    [HttpGet("salarie")]
    [Authorize(Roles = "Salarie")]
    public async Task<IActionResult> ObtenirParSalarie()
    {
        var _codes = await _sCode.ObtenirParSalarie(ObtenirUtilisateurId());
        return Ok(_codes);
    }

    [HttpPost("valider")]
    [Authorize(Roles = "Fournisseur")]
    public async Task<IActionResult> Valider([FromBody] DTO_ValiderCode p_dto)
    {
        var (_succes, _message, _resultat) = await _sCode.Valider(p_dto.Valeur, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(_resultat);
        return Ok(_resultat);
    }

    [HttpPost("revoquer/{p_id}")]
    [Authorize(Roles = "Patron")]
    public async Task<IActionResult> Revoquer(int p_id)
    {
        var (_succes, _message) = await _sCode.Revoquer(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
