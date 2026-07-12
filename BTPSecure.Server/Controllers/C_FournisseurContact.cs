using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/fournisseurs-contacts")]
[Authorize(Roles = "Dirigeant")]
public class C_FournisseurContact : ControllerBase
{
    private readonly S_FournisseurContact _service;

    public C_FournisseurContact(S_FournisseurContact p_service)
    {
        _service = p_service;
    }

    private int ObtenirDirigeantId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet]
    public async Task<IActionResult> Lister()
    {
        var _liste = await _service.Lister(ObtenirDirigeantId());
        return Ok(_liste);
    }

    [HttpPost("creer")]
    public async Task<IActionResult> Creer([FromBody] DTO_CreerFournisseurContact p_dto)
    {
        var (_succes, _message, _contact) = await _service.Creer(p_dto, ObtenirDirigeantId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_contact);
    }

    [HttpPost("modifier/{p_id}")]
    public async Task<IActionResult> Modifier(int p_id, [FromBody] DTO_CreerFournisseurContact p_dto)
    {
        var (_succes, _message) = await _service.Modifier(p_id, p_dto, ObtenirDirigeantId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("supprimer/{p_id}")]
    public async Task<IActionResult> Supprimer(int p_id)
    {
        var (_succes, _message) = await _service.Supprimer(p_id, ObtenirDirigeantId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
