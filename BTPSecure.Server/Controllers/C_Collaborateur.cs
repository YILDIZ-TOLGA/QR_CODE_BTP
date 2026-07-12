using System.Security.Claims;
using BTPSecure.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/collaborateur")]
[Authorize(Roles = "Collaborateur")]
public class C_Collaborateur : ControllerBase
{
    private readonly S_Invitation _sInvitation;

    public C_Collaborateur(S_Invitation p_sInvitation)
    {
        _sInvitation = p_sInvitation;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("invitations")]
    public async Task<IActionResult> ObtenirInvitations()
    {
        var _invitations = await _sInvitation.ObtenirInvitations(ObtenirUtilisateurId());
        return Ok(_invitations);
    }

    [HttpPost("accepter/{p_id}")]
    public async Task<IActionResult> Accepter(int p_id)
    {
        var (_succes, _message) = await _sInvitation.Accepter(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("refuser/{p_id}")]
    public async Task<IActionResult> Refuser(int p_id)
    {
        var (_succes, _message) = await _sInvitation.Refuser(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("quitter/{p_id}")]
    public async Task<IActionResult> Quitter(int p_id)
    {
        var (_succes, _message) = await _sInvitation.Quitter(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
