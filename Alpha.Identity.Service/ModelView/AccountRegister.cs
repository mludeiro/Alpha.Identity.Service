using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alpha.Identity.ModelView;

public class AccountRegister
{
    [Required]
    [JsonPropertyName("userName")]
    public string? Username {get; set;}

    [Required]
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [Required]
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
    
    [Required]
    [JsonPropertyName("email")]
    public string? Email {get; set;}

    [Required]
    [JsonPropertyName("password")]
    public string? Password {get;set;}
}