using System.Security.Claims;
using BTPSecure.Server.Services;
using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Route("api/codes")]
[Authorize]
public class C_Code : ControllerBase
{
    private readonly S_Code _sCode;

    public C_Code(S_Code p_sCode)
    {
        _sCode = p_sCode;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpPost("creer")]
    // Dirigeant OU Responsable Admin (vérif fine du rôle-entreprise faite dans S_Code.Creer)
    [Authorize(Roles = "Dirigeant,Collaborateur")]
    public async Task<IActionResult> Creer([FromBody] DTO_CreerCode p_dto)
    {
        var (_succes, _message, _code) = await _sCode.Creer(p_dto, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(_code);
    }

    [HttpGet("dirigeant")]
    [Authorize(Roles = "Dirigeant")]
    public async Task<IActionResult> ObtenirParDirigeant()
    {
        var _codes = await _sCode.ObtenirParDirigeant(ObtenirUtilisateurId());
        return Ok(_codes);
    }

    [HttpGet("collaborateur")]
    [Authorize(Roles = "Collaborateur")]
    public async Task<IActionResult> ObtenirParCollaborateur()
    {
        var _codes = await _sCode.ObtenirParCollaborateur(ObtenirUtilisateurId());
        return Ok(_codes);
    }

    [HttpPost("valider")]
    [Authorize(Roles = "Fournisseur")]
    public async Task<IActionResult> Valider([FromBody] DTO_ValiderCode p_dto)
    {
        var (_succes, _message, _resultat) = await _sCode.Valider(p_dto.Valeur, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(_resultat);
        return Ok(_resultat);
    }

    [HttpGet("commandes-a-venir")]
    [Authorize(Roles = "Fournisseur")]
    public async Task<IActionResult> ObtenirCommandesAVenir()
    {
        var _liste = await _sCode.ObtenirCommandesAVenir(ObtenirUtilisateurId());
        return Ok(_liste);
    }

    [HttpPost("valider-pour-commande")]
    [Authorize(Roles = "Fournisseur")]
    public async Task<IActionResult> ValiderPourCommande([FromBody] DTO_ValiderPourCommande p_dto)
    {
        var (_succes, _message, _resultat) = await _sCode.ValiderPourCommande(p_dto.CodeId, p_dto.Valeur, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(_resultat);
        return Ok(_resultat);
    }

    [HttpPost("marquer-prete/{p_codeId}")]
    [Authorize(Roles = "Fournisseur")]
    public async Task<IActionResult> MarquerPrete(int p_codeId)
    {
        var (_succes, _message) = await _sCode.MarquerPrete(p_codeId, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }

    [HttpGet("notifications-dirigeant")]
    [Authorize(Roles = "Dirigeant")]
    public async Task<IActionResult> ObtenirNotificationsDirigeant()
    {
        var _liste = await _sCode.ObtenirNotificationsDirigeant(ObtenirUtilisateurId());
        return Ok(_liste);
    }

    [HttpPost("revoquer/{p_id}")]
    [Authorize(Roles = "Dirigeant")]
    public async Task<IActionResult> Revoquer(int p_id)
    {
        var (_succes, _message) = await _sCode.Revoquer(p_id, ObtenirUtilisateurId());
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
