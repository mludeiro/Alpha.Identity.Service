using Alpha.Common.Token;
using Alpha.Identity.Data;
using Alpha.Identity.Model;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Alpha.Identity.Services;

public interface IIdentityTokenService
{
    Task<TokenGeneration> GenerateToken(AlphaUser user);
}

public class IdentityTokenService(UserManager<AlphaUser> userManager, DataContext dataContext, IRestTokenService restTokenService) : IIdentityTokenService
{
    public async Task<TokenGeneration> GenerateToken(AlphaUser user)
    {
        var claims = new List<ClaimValue>()
        {
            new() { Type = ClaimTypes.NameIdentifier, Value = user.Id},
            new() { Type = ClaimTypes.Name, Value = user.UserName},
            new() { Type = ClaimTypes.GivenName, Value =  user.FirstName ?? string.Empty},
            new() { Type = ClaimTypes.Surname, Value = user.LastName ?? string.Empty},
            new() { Type = ClaimTypes.Email, Value = user.Email!},
            new() { Type = JwtRegisteredClaimNames.Jti, Value = Guid.NewGuid().ToString()} // TODO move to token service
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
            PolicyClaim.Values.ForEach( c => claims.Add(new() { Type =c.Type, Value = "true" }));
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

        var tokenResponse = await restTokenService.PostAsync(claims);
        return tokenResponse!;
    }

}
