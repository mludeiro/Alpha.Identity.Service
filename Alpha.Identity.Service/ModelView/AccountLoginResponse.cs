using System.Text.Json.Serialization;

namespace Alpha.Identity.ModelView;

public class AccountLoginResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }
}