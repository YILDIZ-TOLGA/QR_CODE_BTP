namespace BTPSecure.Shared.DTOs;

public class DTO_EntrepriseAffichage
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Adresse { get; set; }
    public string? Siret { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstAutorisee { get; set; }
}
