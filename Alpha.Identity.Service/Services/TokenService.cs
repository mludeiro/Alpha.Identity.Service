using Alpha.Identity.Data;
using Alpha.Identity.DTO;
using Alpha.Identity.Model;
using Alpha.Identity.ModelView;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Alpha.Identity.Services;

public interface ITokenService
{
    Task<JwtSecurityToken> GenerateToken(IdentityUser user);
    Task<RefreshToken> GenerateRefreshToken( JwtSecurityToken token, IdentityUser user);
    string SerializeToken(JwtSecurityToken token);

}

public class TokenService(UserManager<IdentityUser> userManager, DataContext dataContext, JwtOptions jwtOptions) : ITokenService
{
    public async Task<JwtSecurityToken> GenerateToken(IdentityUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var userClaims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        //Add User Role Claims
        var userRoles = await userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles)
        {
            userClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: userClaims,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: credentials
            );

//        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
        return token;
    }

    public string SerializeToken(JwtSecurityToken token) => new JwtSecurityTokenHandler().WriteToken(token);

    public async Task<RefreshToken> GenerateRefreshToken( JwtSecurityToken token, IdentityUser user)
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

}
