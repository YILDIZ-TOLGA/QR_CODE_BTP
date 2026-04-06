namespace BTPSecure.Shared.Entites;

public class E_Entreprise
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Adresse { get; set; }
    public string? Siret { get; set; }
    public DateTime DateCreation { get; set; }
    public int PatronId { get; set; }
    public E_Utilisateur Patron { get; set; } = null!;
}
