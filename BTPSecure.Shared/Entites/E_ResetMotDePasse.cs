namespace BTPSecure.Shared.Entites;

public class E_ResetMotDePasse
{
    public int Id { get; set; }
    public int UtilisateurId { get; set; }
    public E_Utilisateur Utilisateur { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public DateTime DateExpiration { get; set; }
    public bool EstUtilise { get; set; }
}
