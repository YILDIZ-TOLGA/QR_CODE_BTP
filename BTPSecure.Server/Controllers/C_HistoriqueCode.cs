using System.Security.Claims;
using BTPSecure.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

// Réservé au DIRIGEANT : un Responsable Admin ne doit pas surveiller ses collègues.
// Le service borne en plus chaque lecture à l'entreprise dont il est propriétaire.
[ApiController]
[Route("api/historique-codes")]
[Authorize(Roles = "Dirigeant")]
public class C_HistoriqueCode : ControllerBase
{
    private readonly S_HistoriqueCode _service;

    public C_HistoriqueCode(S_HistoriqueCode p_service)
    {
        _service = p_service;
    }

    private int ObtenirUtilisateurId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("porteurs")]
    public async Task<IActionResult> ObtenirPorteurs()
    {
        var _liste = await _service.ObtenirPorteurs(ObtenirUtilisateurId());
        return Ok(_liste);
    }

    [HttpGet("utilisations/{p_porteurId}")]
    public async Task<IActionResult> ObtenirUtilisations(int p_porteurId)
    {
        var _liste = await _service.ObtenirUtilisations(ObtenirUtilisateurId(), p_porteurId);
        return Ok(_liste);
    }
}
