using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/entreprises")]
[Authorize]
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
    [Authorize(Roles = "Dirigeant")]
    public async Task<IActionResult> Creer([FromBody] DTO_CreerEntreprise p_dto)
    {
        var (_succes, _message, _entreprise) = await _sEntreprise.Creer(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_entreprise);
    }

    [HttpGet("ma-entreprise")]
    [Authorize(Roles = "Dirigeant")]
    public async Task<IActionResult> ObtenirMonEntreprise()
    {
        var _entreprise = await _sEntreprise.ObtenirParDirigeant(ObtenirUtilisateurId());
        return Ok(_entreprise);
    }

    [HttpPost("ajouter-collaborateur")]
    [Authorize(Roles = "Dirigeant")]
    public async Task<IActionResult> AjouterCollaborateur([FromBody] DTO_AjouterCollaborateur p_dto)
    {
        var (_succes, _message, _collaborateur) = await _sEntreprise.AjouterCollaborateur(p_dto.Email, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_collaborateur);
    }

    // Dirigeant OU Responsable Admin (contrôle fin dans le service)
    [HttpPost("creer-collaborateur")]
    [Authorize(Roles = "Dirigeant,Collaborateur")]
    public async Task<IActionResult> CreerCollaborateur([FromBody] DTO_CreerCollaborateur p_dto)
    {
        var (_succes, _message) = await _sEntreprise.CreerCollaborateur(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpGet("collaborateurs")]
    [Authorize(Roles = "Dirigeant")]
    public async Task<IActionResult> ObtenirCollaborateurs()
    {
        var _collaborateurs = await _sEntreprise.ObtenirCollaborateurs(ObtenirUtilisateurId());
        return Ok(_collaborateurs);
    }

    [HttpDelete("retirer-collaborateur/{p_id}")]
    [Authorize(Roles = "Dirigeant")]
    public async Task<IActionResult> RetirerCollaborateur(int p_id)
    {
        var (_succes, _message) = await _sEntreprise.RetirerCollaborateur(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("changer-role/{p_collaborateurId}")]
    [Authorize(Roles = "Dirigeant")]
    public async Task<IActionResult> ChangerRole(int p_collaborateurId, [FromBody] DTO_ChangerRole p_dto)
    {
        var (_succes, _message) = await _sEntreprise.ChangerRole(p_collaborateurId, p_dto.RoleEntreprise, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
