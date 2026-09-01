using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_Notification
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Enum_SeveriteNotification Severite { get; set; }
    public DateTime DateCreation { get; set; }
}
