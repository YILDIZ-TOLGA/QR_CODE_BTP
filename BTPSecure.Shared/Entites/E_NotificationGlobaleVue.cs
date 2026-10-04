namespace BTPSecure.Shared.Entites;

// Trace la PREMIÈRE fois qu'un utilisateur a vu une diffusion.
// ⚠️ Ne sert PAS à masquer le message ensuite : une diffusion revient à chaque
// connexion tant que sa période court. Cette table ne mesure que la portée — combien
// de destinataires distincts l'ont vue au moins une fois.
public class E_NotificationGlobaleVue
{
    public int Id { get; set; }

    public int NotificationGlobaleId { get; set; }
    public E_NotificationGlobale NotificationGlobale { get; set; } = null!;

    public int UtilisateurId { get; set; }
    public E_Utilisateur Utilisateur { get; set; } = null!;

    public DateTime DateVue { get; set; }
}
