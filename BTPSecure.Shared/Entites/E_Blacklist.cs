namespace BTPSecure.Shared.Entites;

public class E_Blacklist
{
    public int Id { get; set; }
    public int FournisseurId { get; set; }
    public E_Utilisateur Fournisseur { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
}
