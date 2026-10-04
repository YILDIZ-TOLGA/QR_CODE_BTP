using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

// Création d'une diffusion par l'admin.
// ⚠️ DateDebut / DateFin doivent arriver en UTC : le navigateur convertit avant l'envoi
// (c'est le fuseau de l'admin qui fait foi, pas celui du serveur Railway).
public class DTO_CreerNotificationGlobale
{
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Enum_SeveriteNotification Severite { get; set; }
    public List<string> RolesCibles { get; set; } = new List<string>();
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
}

// Une diffusion telle que l'admin la voit dans sa liste
public class DTO_NotificationGlobaleAdmin
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Enum_SeveriteNotification Severite { get; set; }
    public List<string> RolesCibles { get; set; } = new List<string>();
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public bool EstActive { get; set; }
    public DateTime DateCreation { get; set; }

    // « AVenir », « EnCours » ou « Terminee » — calculé côté serveur pour que
    // l'affichage ne dépende pas de l'horloge du navigateur.
    public string Etat { get; set; } = string.Empty;

    // Combien de comptes l'ont déjà vue, et combien sont concernés
    public int NombreVues { get; set; }
    public int NombreDestinataires { get; set; }
}
