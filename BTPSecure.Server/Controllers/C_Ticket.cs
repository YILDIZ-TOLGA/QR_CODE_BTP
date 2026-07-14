using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class C_Ticket : ControllerBase
{
    private readonly S_Ticket _service;

    public C_Ticket(S_Ticket p_service)
    {
        _service = p_service;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("annuaire")]
    public async Task<IActionResult> Annuaire()
    {
        var _liste = await _service.ObtenirAnnuaire(ObtenirUtilisateurId());
        return Ok(_liste);
    }

    [HttpPost("envoyer")]
    public async Task<IActionResult> Envoyer([FromBody] DTO_EnvoyerTicket p_dto)
    {
        var (_succes, _message) = await _service.Envoyer(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpGet("recus")]
    public async Task<IActionResult> Recus()
    {
        var _liste = await _service.ObtenirRecus(ObtenirUtilisateurId());
        return Ok(_liste);
    }

    [HttpGet("envoyes")]
    public async Task<IActionResult> Envoyes()
    {
        var _liste = await _service.ObtenirEnvoyes(ObtenirUtilisateurId());
        return Ok(_liste);
    }

    [HttpGet("non-lus")]
    public async Task<IActionResult> NonLus()
    {
        var _nombre = await _service.CompterNonLus(ObtenirUtilisateurId());
        return Ok(new { count = _nombre });
    }

    [HttpPost("marquer-lu/{p_id}")]
    public async Task<IActionResult> MarquerLu(int p_id)
    {
        var (_succes, _message) = await _service.MarquerLu(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpGet("piece-jointe/{p_id}")]
    public async Task<IActionResult> PieceJointe(int p_id)
    {
        var _pj = await _service.ObtenirPieceJointe(p_id, ObtenirUtilisateurId());
        if (_pj == null) return NotFound();
        return Ok(_pj);
    }
}
