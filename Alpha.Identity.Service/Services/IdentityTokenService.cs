using Alpha.Common.TokenService;
using Alpha.Identity.Data;
using Alpha.Identity.Model;
using Alpha.Tools.Security;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Alpha.Identity.Services;

public interface IIdentityTokenService
{
    Task<List<ClaimValue>> GenerateTokenClaims(AlphaUser user);
}

public class IdentityTokenService(UserManager<AlphaUser> userManager, DataContext dataContext) : IIdentityTokenService
{
    public async Task<List<ClaimValue>> GenerateTokenClaims(AlphaUser user)
    {
        var claims = new List<ClaimValue>()
        {
            new() { Type = ClaimTypes.GivenName, Value =  user.FirstName ?? string.Empty },
            new() { Type = ClaimTypes.Surname, Value = user.LastName ?? string.Empty },
            new() { Type = ClaimTypes.Email, Value = user.Email! }
        };

        var userClaims = await userManager.GetClaimsAsync(user);
        if( userClaims != null )
        {
            foreach (var userClaim in userClaims)
            {
                claims.Add(new() { Type = userClaim.Type, Value = "true" });
            }
        }

        //Add User Role Claims
        if(user.IsAdmin)
        {
            foreach(var claim in PolicyClaim.Values)
            {
                claims.Add(new() { Type = claim.Type, Value = "true" });
            }
        }
        else
        {
            var rolenames = await userManager.GetRolesAsync(user);

            var roleClaims = from userRole in rolenames
                 join role in dataContext.Roles on userRole equals role.Name
                 join roleClaim in dataContext.RoleClaims on role.Id equals roleClaim.RoleId
                 select roleClaim.ClaimType;

            foreach (var roleClaim in roleClaims.Distinct())
            {
                claims.Add( new() { Type = roleClaim, Value = "true" });
            }
        }

        return claims;
    }

}
