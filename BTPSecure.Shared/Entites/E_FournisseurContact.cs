namespace BTPSecure.Shared.Entites;

public class E_FournisseurContact
{
    public int Id { get; set; }
    public int DirigeantId { get; set; }
    public E_Utilisateur Dirigeant { get; set; } = null!;
    public string NomEntreprise { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Siret { get; set; } = string.Empty;
    public string? Siren { get; set; }
    public DateTime DateCreation { get; set; }
}
