using BTPSecure.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTPSecure.Server.Controllers;

// Portail d'accès de la phase de test (beta).
// Actif UNIQUEMENT tant que la variable d'env CODE_ACCES est définie sur le serveur.
// La retirer de Railway désactive le portail (fin de la beta) sans aucune modification de code.
[ApiController]
[Route("api/acces")]
[AllowAnonymous]
public class C_Acces : ControllerBase
{
    // Le portail est-il actif ? (vrai seulement si CODE_ACCES est renseignée)
    [HttpGet("statut")]
    public IActionResult Statut()
    {
        var _codeAttendu = Environment.GetEnvironmentVariable("CODE_ACCES");
        bool _actif = !string.IsNullOrWhiteSpace(_codeAttendu);
        return Ok(new { actif = _actif });
    }

    // Vérifie le code saisi. Si le portail est inactif, tout est accepté.
    [HttpPost("verifier")]
    public IActionResult Verifier([FromBody] DTO_CodeAcces p_dto)
    {
        var _codeAttendu = Environment.GetEnvironmentVariable("CODE_ACCES");

        if (string.IsNullOrWhiteSpace(_codeAttendu))
        {
            return Ok(new { valide = true });
        }

        if (p_dto == null || string.IsNullOrWhiteSpace(p_dto.Code))
        {
            return Unauthorized(new { message = "Code d'accès invalide." });
        }

        if (string.Equals(p_dto.Code.Trim(), _codeAttendu.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new { valide = true });
        }

        return Unauthorized(new { message = "Code d'accès invalide." });
    }
}
