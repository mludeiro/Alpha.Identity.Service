using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alpha.Identity.ModelView;

public class AccountLogin
{
    [Required]
    [JsonPropertyName("user")]
    public string? Name { get; set; }

    [Required]
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}