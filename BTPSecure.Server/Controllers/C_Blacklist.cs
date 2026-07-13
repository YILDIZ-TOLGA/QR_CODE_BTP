using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/blacklist")]
[Authorize(Roles = "Fournisseur")]
public class C_Blacklist : ControllerBase
{
    private readonly S_Blacklist _service;

    public C_Blacklist(S_Blacklist p_service)
    {
        _service = p_service;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet]
    public async Task<IActionResult> Lister()
    {
        var _liste = await _service.Lister(ObtenirUtilisateurId());
        return Ok(_liste);
    }

    [HttpPost("ajouter")]
    public async Task<IActionResult> Ajouter([FromBody] DTO_AjouterBlacklist p_dto)
    {
        var (_succes, _message) = await _service.Ajouter(p_dto.Email, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("supprimer/{p_id}")]
    public async Task<IActionResult> Supprimer(int p_id)
    {
        var (_succes, _message) = await _service.Supprimer(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
