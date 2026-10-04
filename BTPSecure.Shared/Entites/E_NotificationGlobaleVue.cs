namespace BTPSecure.Shared.Entites;

// Trace qu'un utilisateur a déjà vu une diffusion : sans elle, le message réapparaîtrait
// à chaque connexion pendant toute la fenêtre de dates, ce qui serait du harcèlement.
public class E_NotificationGlobaleVue
{
    public int Id { get; set; }

    public int NotificationGlobaleId { get; set; }
    public E_NotificationGlobale NotificationGlobale { get; set; } = null!;

    public int UtilisateurId { get; set; }
    public E_Utilisateur Utilisateur { get; set; } = null!;

    public DateTime DateVue { get; set; }
}
