using System.Security.Claims;
using Alpha.Common.TokenService;
using Alpha.Identity.Model;
using Alpha.Identity.ModelView;
using Alpha.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace Alpha.Identity.Controllers;

[Route("/api/account")]
public class AccountController(UserManager<AlphaUser> userManager, IIdentityTokenService identityTokenService, 
        ITokenService tokenService, IHttpContextAccessor httpContext) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AccountRegister register)
    {
        if( !ModelState.IsValid )
            return BadRequest("Missing required fields");

        var user = await userManager.FindByNameAsync(register.Username!);
        if (user is not null)
            return BadRequest("User already registered");

        user = await userManager.FindByEmailAsync(register.Email!);
        if (user is not null)
            return BadRequest("Email already registered");
        
        user = new AlphaUser(register.Username!) 
        {
            Email = register.Email,
            FirstName = register.FirstName,
            LastName = register.LastName,
            IsAdmin = "admin".Equals(register.Username, StringComparison.OrdinalIgnoreCase)
        };


        var operationResult = await userManager.CreateAsync(user, register.Password!);
        await userManager.AddClaimAsync(user, new Claim( PolicyClaim.identityUserMe, "true"));

        return operationResult is not null && operationResult.Succeeded ? Ok() : StatusCode(500, operationResult?.Errors);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AccountLogin login)
    {
        if( !ModelState.IsValid )
        {
            return BadRequest("Missing required fields");
        }

        var user = await userManager.FindByNameAsync(login.Name!);
        if (user is null || !await userManager.CheckPasswordAsync(user, login.Password!))
        {
            return Unauthorized("Invalid username/password");
        }

        var claims = await identityTokenService.GenerateTokenClaims(user);

        var tokenResponse = await tokenService.GetToken(claims);

        if( tokenResponse.StatusCode == System.Net.HttpStatusCode.BadGateway )
            return Unauthorized("Cant connect to token service");

        if( !tokenResponse.IsSuccessStatusCode )
            return Unauthorized("Token service error");

        return Ok(tokenResponse.Content);
    }

    // [AllowAnonymous]
    // [HttpPost("refresh")]
    // public async Task<IActionResult> RefreshToken([FromBody] AccountRefresh tokenRequest)
    // {
    //     if (tokenRequest?.RefreshToken is null)
    //         return BadRequest("Please, provide all required fields");

    //     var jwttoken = await TokenService.GenerateToken(tokenRequest.RefreshToken);

    //     if(jwttoken is null)
    //         return Unauthorized();

    //     var token = TokenService.SerializeToken(jwttoken);
    //     return Ok( new AccountLoginResponse { Token = token, RefreshToken = tokenRequest.RefreshToken });
    // }

    [Authorize(AuthenticationSchemes = "Bearer", Policy = PolicyClaim.identityUserMe)]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var username = httpContext.HttpContext?.User.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        if( username is null )
        {
            return NotFound("Claim not found");
        }

        var user = await userManager.FindByNameAsync(username);
        if(user is null)
        {
            return NotFound("User not found");
        }

        return Ok(user);
    }

}