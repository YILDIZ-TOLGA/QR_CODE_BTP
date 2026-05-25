namespace BTPSecure.Shared.DTOs;

public class DTO_DemandeReset
{
    public string Email { get; set; } = string.Empty;
}

public class DTO_ReinitialiserMotDePasse
{
    public string Token { get; set; } = string.Empty;
    public string NouveauMotDePasse { get; set; } = string.Empty;
}
