using System.Security.Claims;

namespace Alpha.Identity.Model;

public class PolicyClaim(string type, string value)
{
    public const string identityUserMe = "Identity.User.Me";

    public static readonly List<PolicyClaim> Values = [
        new PolicyClaim(identityUserMe, "Read or Write My User"),
        new PolicyClaim("Identity.User.Read", "Read Users"),
        new PolicyClaim("Identity.User.Write", "Create or Update Users"),
        new PolicyClaim("Weather.Weather.Read", "Read Weather")
    ];

    public string Type = type;

    public string Value = value;

    public Claim ToClaim()
    {
        return new Claim(Type, "true");
    }
}