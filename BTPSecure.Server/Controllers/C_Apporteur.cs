using System.Security.Claims;
using BTPSecure.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

// Espace de l'apporteur d'affaires. Chaque lecture est bornée à l'identifiant du
// jeton : un apporteur ne voit jamais les filleuls d'un autre.
[ApiController]
[Route("api/apporteur")]
[Authorize(Roles = "ApporteurAffaire")]
public class C_Apporteur : ControllerBase
{
    private readonly S_Apporteur _service;

    public C_Apporteur(S_Apporteur p_service)
    {
        _service = p_service;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("tableau-de-bord")]
    public async Task<IActionResult> ObtenirTableauDeBord()
    {
        var _tableau = await _service.ObtenirTableauDeBord(ObtenirUtilisateurId());
        return Ok(_tableau);
    }
}
