using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/entreprises")]
[Authorize(Roles = "Patron")]
public class C_Entreprise : ControllerBase
{
    private readonly S_Entreprise _sEntreprise;

    public C_Entreprise(S_Entreprise p_sEntreprise)
    {
        _sEntreprise = p_sEntreprise;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpPost("creer")]
    public async Task<IActionResult> Creer([FromBody] DTO_CreerEntreprise p_dto)
    {
        var (_succes, _message, _entreprise) = await _sEntreprise.Creer(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_entreprise);
    }

    [HttpGet("ma-entreprise")]
    public async Task<IActionResult> ObtenirMonEntreprise()
    {
        var _entreprise = await _sEntreprise.ObtenirParPatron(ObtenirUtilisateurId());
        return Ok(_entreprise);
    }

    [HttpPost("ajouter-salarie")]
    public async Task<IActionResult> AjouterSalarie([FromBody] DTO_AjouterSalarie p_dto)
    {
        var (_succes, _message, _salarie) = await _sEntreprise.AjouterSalarie(p_dto.Email, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_salarie);
    }

    [HttpGet("salaries")]
    public async Task<IActionResult> ObtenirSalaries()
    {
        var _salaries = await _sEntreprise.ObtenirSalaries(ObtenirUtilisateurId());
        return Ok(_salaries);
    }

    [HttpDelete("retirer-salarie/{p_id}")]
    public async Task<IActionResult> RetirerSalarie(int p_id)
    {
        var (_succes, _message) = await _sEntreprise.RetirerSalarie(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
