using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.Entites;

// Message diffusé par un administrateur à tous les comptes d'un ou plusieurs rôles,
// affiché à leur connexion pendant une fenêtre de dates.
//
// ⚠️ Volontairement UNE ligne, et non une notification copiée pour chaque compte :
//  - la fenêtre de dates reste vraie (une copie existerait déjà avant la date de début) ;
//  - un compte créé APRÈS la diffusion la reçoit quand même, tant qu'elle est active ;
//  - modifier ou désactiver la diffusion agit immédiatement sur tout le monde.
// Le suivi « qui l'a vue » vit dans E_NotificationGlobaleVue.
public class E_NotificationGlobale
{
    public int Id { get; set; }

    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Enum_SeveriteNotification Severite { get; set; }

    // Rôles destinataires, séparés par des virgules (ex. « Dirigeant,Fournisseur »).
    // Le filtrage se fait en mémoire : les diffusions actives se comptent sur les doigts
    // d'une main, un LIKE sur une liste concaténée serait fragile pour rien.
    public string RolesCibles { get; set; } = string.Empty;

    // Bornes en UTC. DateFin est stockée à la fin de la journée choisie : l'admin
    // sélectionne un jour, il l'entend inclus.
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }

    // Permet de couper une diffusion sans la supprimer ni perdre qui l'a déjà vue
    public bool EstActive { get; set; } = true;

    public DateTime DateCreation { get; set; }

    // Admin auteur. Nullable pour que la diffusion survive à la suppression d'un compte.
    public int? CreateurId { get; set; }
    public E_Utilisateur? Createur { get; set; }
}
