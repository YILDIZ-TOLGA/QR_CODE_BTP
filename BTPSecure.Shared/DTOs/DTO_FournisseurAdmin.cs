namespace BTPSecure.Shared.DTOs;

public class DTO_FournisseurAdmin
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? NomSociete { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Siret { get; set; }
    public string? Siren { get; set; }
    public string? Telephone { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstValide { get; set; }
    public bool EstActif { get; set; }
    // Un sous-compte n'a pas de limite propre : elle se règle sur le compte principal
    public bool EstSousCompte { get; set; }
    public int LimiteSousComptes { get; set; }
    // Compte principal : nombre de sous-comptes qui suivront un blocage
    public int NombreSousComptes { get; set; }
    // Sous-compte : son compte principal est bloqué (impossible de le débloquer seul)
    public bool ParentBloque { get; set; }
    public string? NomParent { get; set; }
}
