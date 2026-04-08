namespace BTPSecure.Shared.DTOs;

public class DTO_EntrepriseAdmin
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Siret { get; set; }
    public string NomPatron { get; set; } = string.Empty;
    public string PrenomPatron { get; set; } = string.Empty;
    public string EmailPatron { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public bool EstAutorisee { get; set; }
    public int NombreSalaries { get; set; }
    public int NombreCodes { get; set; }
}
