namespace BTPSecure.Shared.DTOs;

public class DTO_EntrepriseAdmin
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Siret { get; set; }
    public string NomDirigeant { get; set; } = string.Empty;
    public string PrenomDirigeant { get; set; } = string.Empty;
    public string EmailDirigeant { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public bool EstAutorisee { get; set; }
    public int NombreCollaborateurs { get; set; }
    public int NombreCodes { get; set; }
    // Plafond commun Responsable + Responsable Admin, et son occupation actuelle
    public int LimiteResponsables { get; set; }
    public int NombreResponsables { get; set; }
    // Parrainage du dirigeant, pour que l'admin sache s'il doit fixer une commission
    // au moment ou il autorise l'entreprise.
    public bool EstParrainee { get; set; }
    public string? NomApporteur { get; set; }
    public string? CodeParrainageUtilise { get; set; }
    // Vrai si la commission a deja ete fixee : on ne la redemande jamais (pas de double paiement)
    public bool ParrainageDejaValide { get; set; }
    public decimal MontantCommission { get; set; }
}
