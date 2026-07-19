namespace BTPSecure.Shared.Entites;

// Pense-bête personnel : chaque mémo appartient à un seul utilisateur.
// Contrairement aux tickets, un mémo n'expire pas.
public class E_Memo
{
    public int Id { get; set; }
    public int UtilisateurId { get; set; }
    public E_Utilisateur Utilisateur { get; set; } = null!;
    public string Titre { get; set; } = string.Empty;
    public string Contenu { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public DateTime DateModification { get; set; }
}
