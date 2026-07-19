namespace BTPSecure.Shared.DTOs;

public class DTO_Memo
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Contenu { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public DateTime DateModification { get; set; }
}

// Création (Id null) ou modification (Id renseigné)
public class DTO_EnregistrerMemo
{
    public int? Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Contenu { get; set; } = string.Empty;
}
