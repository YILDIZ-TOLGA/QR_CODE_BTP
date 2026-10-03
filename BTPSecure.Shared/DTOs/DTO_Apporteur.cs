namespace BTPSecure.Shared.DTOs;

// Une personne amenée par l'apporteur, telle qu'il la voit dans son tableau de bord.
// ⚠️ Volontairement sans email ni téléphone du filleul : l'apporteur a droit au suivi
// de sa commission, pas aux coordonnées de ses filleuls (minimisation RGPD).
public class DTO_FilleulAffichage
{
    public int ParrainageId { get; set; }
    public string NomFilleul { get; set; } = string.Empty;
    public string PrenomFilleul { get; set; } = string.Empty;
    public string? NomEntreprise { get; set; }
    public DateTime DateInscription { get; set; }
    public bool EstValide { get; set; }
    public decimal MontantCommission { get; set; }
    public DateTime? DateValidation { get; set; }
}

// Tableau de bord complet d'un apporteur d'affaires
public class DTO_TableauApporteur
{
    public string CodeParrainage { get; set; } = string.Empty;
    public int NombreFilleuls { get; set; }
    public int NombreEnAttente { get; set; }
    public int NombreValides { get; set; }
    // Somme des commissions des parrainages validés
    public decimal TotalCumule { get; set; }
    public List<DTO_FilleulAffichage> Filleuls { get; set; } = new List<DTO_FilleulAffichage>();
}

// Vue admin d'un apporteur : de quoi savoir qui payer, et combien
public class DTO_ApporteurAdmin
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CodeParrainage { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public bool EstActif { get; set; }
    public int NombreFilleuls { get; set; }
    public int NombreEnAttente { get; set; }
    public decimal TotalCumule { get; set; }
}

// Autorisation d'une entreprise par l'admin. Le montant n'est lu que si l'entreprise
// est parrainée et que le parrainage est encore en attente.
public class DTO_AutoriserEntreprise
{
    public decimal MontantCommission { get; set; }
}
