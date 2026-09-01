namespace BTPSecure.Shared.DTOs;

public class DTO_SousCompte
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstActif { get; set; }
}

public class DTO_CreerSousCompte
{
    public string Email { get; set; } = string.Empty;
    // Optionnels : à défaut, le sous-compte prend le nom de l'entreprise du compte principal
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Telephone { get; set; }
}

// Quota de sous-comptes du fournisseur connecté (affiché sur la page Mes sous-comptes)
public class DTO_QuotaSousComptes
{
    public int Limite { get; set; }
    public int Actifs { get; set; }
}

// Changement de la limite de sous-comptes par l'admin
public class DTO_LimiteSousComptes
{
    public int Limite { get; set; }
}

// Changement du plafond Responsable + Responsable Admin d'une entreprise par l'admin
public class DTO_LimiteResponsables
{
    public int Limite { get; set; }
}
