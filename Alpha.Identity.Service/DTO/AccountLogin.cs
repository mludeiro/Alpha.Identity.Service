using System.Text.Json.Serialization;

namespace Alpha.Identity.Common.DTO;

public class AccountLogin
{
    [JsonPropertyName("email")]
    public string? Email {get; set;}

    [JsonPropertyName("password")]
    public string? Password {get;set;}
}