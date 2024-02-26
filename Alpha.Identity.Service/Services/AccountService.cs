using Alpha.Identity.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Alpha.Identity.Services;

public interface ITokenService
{
    Task<string> GenerateToken(IdentityUser user);
}

public class TokenService(UserManager<IdentityUser> userManager, JwtOptions jwtOptions) : ITokenService
{
    public async Task<string> GenerateToken(IdentityUser user)
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
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
