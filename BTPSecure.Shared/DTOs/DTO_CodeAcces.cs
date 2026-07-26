namespace BTPSecure.Shared.DTOs;

// Code d'accès de la phase de test (beta). Piloté côté serveur par la variable d'env CODE_ACCES.
public class DTO_CodeAcces
{
    public string Code { get; set; } = string.Empty;
}
