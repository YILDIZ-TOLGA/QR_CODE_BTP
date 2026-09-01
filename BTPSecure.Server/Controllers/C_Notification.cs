using System.Security.Claims;
using BTPSecure.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

// Notifications personnelles : chacun ne voit que les siennes
[ApiController]
[Route("api/notifications")]
[Authorize]
public class C_Notification : ControllerBase
{
    private readonly S_Notification _service;

    public C_Notification(S_Notification p_service)
    {
        _service = p_service;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("non-lues")]
    public async Task<IActionResult> ObtenirNonLues()
    {
        var _liste = await _service.ObtenirNonLues(ObtenirUtilisateurId());
        return Ok(_liste);
    }

    [HttpPost("marquer-lues")]
    public async Task<IActionResult> MarquerLues()
    {
        await _service.MarquerLues(ObtenirUtilisateurId());
        return Ok(new { message = "Notifications marquées comme lues." });
    }
}
