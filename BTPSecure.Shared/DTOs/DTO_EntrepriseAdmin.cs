namespace BTPSecure.Shared.DTOs;

public class DTO_EntrepriseAdmin
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Siret { get; set; }
    public string NomDirigeant { get; set; } = string.Empty;
    public string PrenomDirigeant { get; set; } = string.Empty;
    public string EmailDirigeant { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public bool EstAutorisee { get; set; }
    public int NombreCollaborateurs { get; set; }
    public int NombreCodes { get; set; }
    // Plafond commun Responsable + Responsable Admin, et son occupation actuelle
    public int LimiteResponsables { get; set; }
    public int NombreResponsables { get; set; }
}
