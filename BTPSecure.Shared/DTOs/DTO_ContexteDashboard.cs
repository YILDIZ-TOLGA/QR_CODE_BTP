namespace BTPSecure.Shared.DTOs;

public class DTO_ContexteDashboard
{
    public bool AAcces { get; set; }
    public bool EstProprietaire { get; set; }
    public bool EstDirigeantSansEntreprise { get; set; }
    public DTO_EntrepriseAffichage? Entreprise { get; set; }
    public List<DTO_CollaborateurAffichage> Collaborateurs { get; set; } = new();
    public List<DTO_CodeAffichage> Codes { get; set; } = new();
    public List<DTO_FournisseurContact> Fournisseurs { get; set; } = new();
    public int NbNotifications { get; set; }
}
