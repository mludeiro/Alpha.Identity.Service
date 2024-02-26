using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alpha.Identity.Common.DTO;

public class AccountRegister
{
    [Required]
    [JsonPropertyName("user")]
    public string? Name {get; set;}

    [Required]
    [JsonPropertyName("email")]
    public string? Email {get; set;}

    [Required]
    [JsonPropertyName("password")]
    public string? Password {get;set;}
}