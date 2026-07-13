namespace BTPSecure.Shared.DTOs;

public class DTO_ContexteCreationCode
{
    public bool PeutCreer { get; set; }
    public int EntrepriseId { get; set; }
    public string NomEntreprise { get; set; } = string.Empty;
    public bool EstAutorisee { get; set; }
    public List<DTO_CollaborateurAffichage> Collaborateurs { get; set; } = new();
    public List<DTO_FournisseurContact> Fournisseurs { get; set; } = new();
}
