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

        if (_utilisateur.Role == Enum_Role.Patron)
        {
            _profil.Entreprises = await _context.Entreprises
                .Where(e => e.PatronId == _id)
                .Select(e => e.Nom)
                .ToListAsync();
        }
        else if (_utilisateur.Role == Enum_Role.Salarie)
        {
            _profil.Entreprises = await _context.SalariesEntreprises
                .Include(se => se.Entreprise)
                .Where(se => se.SalarieId == _id
                    && se.EstActif
                    && se.StatutInvitation == Enum_StatutInvitation.Acceptee)
                .Select(se => se.Entreprise.Nom)
                .ToListAsync();
        }

        return Ok(_profil);
    }
}
