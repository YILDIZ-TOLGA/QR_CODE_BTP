using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class C_Auth : ControllerBase
{
    private readonly S_Auth _sAuth;

    public C_Auth(S_Auth p_sAuth)
    {
        _sAuth = p_sAuth;
    }

    [HttpPost("inscription")]
    public async Task<IActionResult> Inscription([FromBody] DTO_Inscription p_dto)
    {
        var (_succes, _message, _) = await _sAuth.Inscrire(p_dto);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("connexion")]
    public async Task<IActionResult> Connexion([FromBody] DTO_Connexion p_dto)
    {
        var (_succes, _message, _reponse) = await _sAuth.Connecter(p_dto);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_reponse);
    }
}
