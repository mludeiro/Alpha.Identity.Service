using Alpha.Identity.Data;
using Alpha.Identity.DTO;
using Alpha.Identity.Model;
using Alpha.Identity.ModelView;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Alpha.Identity.Services;

public interface ITokenService
{
    Task<JwtSecurityToken> GenerateToken(AlphaUser user);
    Task<JwtSecurityToken?> GenerateToken(string refreshToken);
    Task<RefreshToken> GenerateRefreshToken( JwtSecurityToken token, AlphaUser user);
    string SerializeToken(JwtSecurityToken token);

}

public class TokenService(UserManager<AlphaUser> userManager, DataContext dataContext, 
    JwtOptions jwtOptions) : ITokenService
{
    public async Task<JwtSecurityToken> GenerateToken(AlphaUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var userClaims = await userManager.GetClaimsAsync(user);
        if( userClaims != null )
        {
            foreach (var userClaim in userClaims)
            {
                claims.Add(new Claim(userClaim.Type, "true"));
            }
        }

        //Add User Role Claims
        if(user.IsAdmin)
        {
            PolicyClaim.Values.ForEach( c => claims.Add(new Claim(c.Type, "true") ));
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
                claims.Add(new Claim(roleClaim, "true"));
            }

            // var roleids = dataContext.Roles.Where( r => rolenames.Contains(r.Name!) ).Select(r => r.Id);
            // var roleclaims = dataContext.RoleClaims.Where( rc => roleids.Contains(rc.RoleId) ).Select(rc => rc.ClaimType).Distinct();

            // foreach( var roleClaim in roleclaims )
            // {
            //     userClaims.Add(new Claim(roleClaim!, "true"));
            // }
        }

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: credentials
            );

        Console.WriteLine( JsonSerializer.Serialize(token) );

        return token;
    }

    public string SerializeToken(JwtSecurityToken token) => new JwtSecurityTokenHandler().WriteToken(token);

    public async Task<RefreshToken> GenerateRefreshToken( JwtSecurityToken token, AlphaUser user)
    {
        var refreshToken = new RefreshToken()
        {
            JwtId = token.Id,
            IsRevoked = false,
            UserId = user.Id,
            DateAdded = DateTime.UtcNow,
            DateExpire = DateTime.UtcNow.AddMonths(6),
            Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
        };
        await dataContext.RefreshTokens.AddAsync(refreshToken);
        await dataContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<JwtSecurityToken?> GenerateToken(string refreshToken)
    {
        var data = dataContext.RefreshTokens.Include(x => x.User).FirstOrDefault( x => x.Token == refreshToken );

        if( data?.User is null )
        {
            return null;
        }

        var token = await GenerateToken(data.User);

        data.JwtId = token.Id;
        await dataContext.SaveChangesAsync();

        return token;
    }
}
