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
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Telephone { get; set; }
}
