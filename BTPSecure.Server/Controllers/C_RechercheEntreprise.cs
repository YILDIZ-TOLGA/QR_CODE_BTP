using BTPSecure.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

// Anonyme : le formulaire d'inscription fournisseur doit pouvoir l'utiliser avant connexion.
// Ne fait que relayer un annuaire public, avec un cache côté serveur.
[ApiController]
[Route("api/recherche-entreprise")]
[AllowAnonymous]
public class C_RechercheEntreprise : ControllerBase
{
    private readonly S_RechercheEntreprise _service;

    public C_RechercheEntreprise(S_RechercheEntreprise p_service)
    {
        _service = p_service;
    }

    [HttpGet("{p_identifiant}")]
    public async Task<IActionResult> Rechercher(string p_identifiant)
    {
        var _info = await _service.Rechercher(p_identifiant);
        return Ok(_info);
    }
}
