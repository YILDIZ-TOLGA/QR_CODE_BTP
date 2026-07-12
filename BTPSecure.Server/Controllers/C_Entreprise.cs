using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/entreprises")]
[Authorize(Roles = "Dirigeant")]
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
        var _entreprise = await _sEntreprise.ObtenirParDirigeant(ObtenirUtilisateurId());
        return Ok(_entreprise);
    }

    [HttpPost("ajouter-collaborateur")]
    public async Task<IActionResult> AjouterCollaborateur([FromBody] DTO_AjouterCollaborateur p_dto)
    {
        var (_succes, _message, _collaborateur) = await _sEntreprise.AjouterCollaborateur(p_dto.Email, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_collaborateur);
    }

    [HttpPost("creer-collaborateur")]
    public async Task<IActionResult> CreerCollaborateur([FromBody] DTO_CreerCollaborateur p_dto)
    {
        var (_succes, _message) = await _sEntreprise.CreerCollaborateur(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpGet("collaborateurs")]
    public async Task<IActionResult> ObtenirCollaborateurs()
    {
        var _collaborateurs = await _sEntreprise.ObtenirCollaborateurs(ObtenirUtilisateurId());
        return Ok(_collaborateurs);
    }

    [HttpDelete("retirer-collaborateur/{p_id}")]
    public async Task<IActionResult> RetirerCollaborateur(int p_id)
    {
        var (_succes, _message) = await _sEntreprise.RetirerCollaborateur(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
