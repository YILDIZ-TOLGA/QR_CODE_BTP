using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

// Pense-bête personnel : accessible à tout utilisateur connecté, chacun ne voyant que ses notes
[ApiController]
[Route("api/memos")]
[Authorize]
public class C_Memo : ControllerBase
{
    private readonly S_Memo _service;

    public C_Memo(S_Memo p_service)
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

    [HttpPost("enregistrer")]
    public async Task<IActionResult> Enregistrer([FromBody] DTO_EnregistrerMemo p_dto)
    {
        var (_succes, _message, _memo) = await _service.Enregistrer(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_memo);
    }

    [HttpPost("supprimer/{p_id}")]
    public async Task<IActionResult> Supprimer(int p_id)
    {
        var (_succes, _message) = await _service.Supprimer(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
