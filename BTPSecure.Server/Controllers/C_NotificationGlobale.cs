using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

// Diffusion de notifications par rôle. Réservé aux administrateurs : c'est un message
// envoyé à tout le monde, personne d'autre ne doit pouvoir en créer.
[ApiController]
[Route("api/notifications-globales")]
[Authorize(Roles = "Admin")]
public class C_NotificationGlobale : ControllerBase
{
    private readonly S_NotificationGlobale _service;

    public C_NotificationGlobale(S_NotificationGlobale p_service)
    {
        _service = p_service;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenirToutes()
    {
        var _liste = await _service.ObtenirToutes();
        return Ok(_liste);
    }

    // Rôles que l'admin peut viser, fournis par le serveur pour que la liste
    // du formulaire ne diverge jamais de la liste blanche de validation.
    [HttpGet("roles")]
    public IActionResult ObtenirRoles()
    {
        return Ok(S_NotificationGlobale.RolesCiblables());
    }

    [HttpPost("creer")]
    public async Task<IActionResult> Creer([FromBody] DTO_CreerNotificationGlobale p_dto)
    {
        var (_succes, _message) = await _service.Creer(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("basculer/{p_id}")]
    public async Task<IActionResult> BasculerActivation(int p_id)
    {
        var (_succes, _message) = await _service.BasculerActivation(p_id);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("supprimer/{p_id}")]
    public async Task<IActionResult> Supprimer(int p_id)
    {
        var (_succes, _message) = await _service.Supprimer(p_id);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
