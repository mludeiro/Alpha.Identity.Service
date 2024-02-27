using System.Text.Json.Serialization;

namespace Alpha.Identity.ModelView;

public class AccountRefresh
{
    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }
}