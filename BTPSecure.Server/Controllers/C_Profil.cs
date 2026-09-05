using System.Security.Claims;
using BTPSecure.Server.Data;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/profil")]
public class C_Profil : ControllerBase
{
    private readonly AppDbContext _context;

    public C_Profil(AppDbContext p_context)
    {
        _context = p_context;
    }

    [HttpGet("moi")]
    public async Task<IActionResult> Moi()
    {
        var _claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(_claimId))
            return Unauthorized();

        var _id = int.Parse(_claimId);
        var _utilisateur = await _context.Utilisateurs.FindAsync(_id);
        if (_utilisateur == null)
            return NotFound();

        var _profil = new DTO_Profil
        {
            Id = _utilisateur.Id,
            Email = _utilisateur.Email,
            Nom = _utilisateur.Nom,
            Prenom = _utilisateur.Prenom,
            Telephone = _utilisateur.Telephone,
            Siret = _utilisateur.Siret,
            Siren = _utilisateur.Siren,
            Role = _utilisateur.Role,
            DateCreation = _utilisateur.DateCreation
        };

        if (_utilisateur.Role == Enum_Role.Dirigeant)
        {
            _profil.Entreprises = await _context.Entreprises
                .Where(e => e.DirigeantId == _id)
                .Select(e => e.Nom)
                .ToListAsync();
        }
        else if (_utilisateur.Role == Enum_Role.Collaborateur)
        {
            _profil.Entreprises = await _context.CollaborateursEntreprises
                .Include(se => se.Entreprise)
                .Where(se => se.CollaborateurId == _id
                    && se.EstActif
                    && se.StatutInvitation == Enum_StatutInvitation.Acceptee)
                .Select(se => se.Entreprise.Nom)
                .ToListAsync();

            // Sert à savoir si la déconnexion automatique pour inactivité s'applique
            _profil.EstResponsable = await _context.CollaborateursEntreprises
                .AnyAsync(se => se.CollaborateurId == _id
                    && se.EstActif
                    && se.StatutInvitation == Enum_StatutInvitation.Acceptee
                    && (se.RoleEntreprise == Enum_RoleEntreprise.Responsable
                        || se.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin));
        }

        return Ok(_profil);
    }

    [HttpPost("changer-mot-de-passe")]
    public async Task<IActionResult> ChangerMotDePasse([FromBody] DTO_ChangerMotDePasse p_dto,
        [FromServices] BTPSecure.Server.Services.S_Auth p_sAuth)
    {
        var _claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(_claimId))
            return Unauthorized();
        var _id = int.Parse(_claimId);

        var (_succes, _message) = await p_sAuth.ChangerMotDePasse(_id, p_dto.AncienMotDePasse, p_dto.NouveauMotDePasse);
        if (!_succes) return BadRequest(new { message = _message });
        return Ok(new { message = _message });
    }
}
