using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_CreerCode
{
    public int CollaborateurId { get; set; }
    public int EntrepriseId { get; set; }
    public string NumeroCommande { get; set; } = string.Empty;
    public Enum_TypeCode TypeCode { get; set; }
    public string? Info { get; set; }
    public string? ListeMateriaux { get; set; }
    public int? DureeValiditeHeures { get; set; }
    public bool UtiliserFournisseur { get; set; } = false;
    public int? FournisseurContactId { get; set; }
    public string? NouveauFournisseurNomEntreprise { get; set; }
    public string? NouveauFournisseurEmail { get; set; }
    public string? NouveauFournisseurSiret { get; set; }
    public string? NouveauFournisseurSiren { get; set; }
}
