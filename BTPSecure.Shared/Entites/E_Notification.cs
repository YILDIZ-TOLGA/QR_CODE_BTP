using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.Entites;

// Notification personnelle : le changement se produit pendant que l'utilisateur est
// déconnecté, on la stocke donc pour l'afficher à sa prochaine connexion, puis on la marque lue.
public class E_Notification
{
    public int Id { get; set; }
    public int UtilisateurId { get; set; }
    public E_Utilisateur Utilisateur { get; set; } = null!;
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Enum_SeveriteNotification Severite { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstLue { get; set; }
}
