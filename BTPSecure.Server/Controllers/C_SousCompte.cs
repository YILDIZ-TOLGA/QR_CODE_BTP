using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/souscompte")]
[Authorize(Roles = "Fournisseur")]
public class C_SousCompte : ControllerBase
{
    private readonly S_SousCompte _service;

    public C_SousCompte(S_SousCompte p_service)
    {
        _service = p_service;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("est-principal")]
    public async Task<IActionResult> EstPrincipal()
    {
        var _est = await _service.EstPrincipal(ObtenirUtilisateurId());
        return Ok(new { estPrincipal = _est });
    }

    [HttpGet]
    public async Task<IActionResult> Lister()
    {
        var _liste = await _service.Lister(ObtenirUtilisateurId());
        return Ok(_liste);
    }

    [HttpPost("creer")]
    public async Task<IActionResult> Creer([FromBody] DTO_CreerSousCompte p_dto)
    {
        var (_succes, _message) = await _service.Creer(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("desactiver/{p_id}")]
    public async Task<IActionResult> Desactiver(int p_id)
    {
        var (_succes, _message) = await _service.Desactiver(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpPost("reactiver/{p_id}")]
    public async Task<IActionResult> Reactiver(int p_id)
    {
        var (_succes, _message) = await _service.Reactiver(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpGet("historique")]
    public async Task<IActionResult> ObtenirHistorique()
    {
        var _liste = await _service.ObtenirHistorique(ObtenirUtilisateurId());
        return Ok(_liste);
    }
}
