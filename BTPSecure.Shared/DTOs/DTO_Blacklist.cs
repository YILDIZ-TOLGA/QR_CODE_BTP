namespace BTPSecure.Shared.DTOs;

public class DTO_Blacklist
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
}

public class DTO_AjouterBlacklist
{
    public string Email { get; set; } = string.Empty;
}
