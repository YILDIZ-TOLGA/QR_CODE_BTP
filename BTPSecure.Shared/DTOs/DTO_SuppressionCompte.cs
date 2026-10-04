namespace BTPSecure.Shared.DTOs;

// Un compte dans la liste administrable
public class DTO_CompteAdmin
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? NomSociete { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstActif { get; set; }
    public bool EstSousCompte { get; set; }
}

// Ce qu'une suppression détruirait, calculé AVANT de la proposer.
// L'admin doit voir l'étendue des dégâts avant de confirmer, pas après.
public class DTO_ApercuSuppression
{
    public int UtilisateurId { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Faux si au moins un blocage s'applique
    public bool EstSupprimable { get; set; }

    // Raisons de refus, formulées pour être lues telles quelles par l'admin
    public List<string> Blocages { get; set; } = new List<string>();

    // Volumes qui seront effacés
    public int NombreCodes { get; set; }
    public int NombreMessages { get; set; }
    public int NombreMemos { get; set; }
    public int NombreNotifications { get; set; }
    public int NombreLiensEntreprise { get; set; }
    public int NombreContactsFournisseur { get; set; }
    public int NombreBlacklist { get; set; }
    public int NombreParrainagesEnAttente { get; set; }

    // Renseigné si le compte est dirigeant : son entreprise disparaît avec lui
    public string? NomEntreprise { get; set; }
    public int NombreCollaborateursDetaches { get; set; }
}

// Confirmation de suppression : l'admin doit retaper l'email du compte vise.
// Controle REFAIT cote serveur (un controle seulement cote navigateur ne protege rien).
public class DTO_ConfirmerSuppression
{
    public string Email { get; set; } = string.Empty;
}
