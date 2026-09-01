namespace BTPSecure.Shared.Entites;

public class E_Entreprise
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Adresse { get; set; }
    public string? Siret { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstAutorisee { get; set; }
    // Plafond commun Responsable + Responsable Admin, réglable par l'admin
    public int LimiteResponsables { get; set; } = 2;
    public int DirigeantId { get; set; }
    public E_Utilisateur Dirigeant { get; set; } = null!;
}
