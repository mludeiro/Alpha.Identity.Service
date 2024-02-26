using System.Security.Claims;
using Alpha.Identity.Common.DTO;
using Alpha.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Alpha.Identity.Controllers;

[Route("/api/account")]
public class AccountController(UserManager<IdentityUser> userManager, ITokenService TokenService, 
        IHttpContextAccessor httpContext) : ControllerBase
{
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] AccountRegister register)
    {
        if( !ModelState.IsValid )
        {
            return BadRequest("Missing required fields");
        }

        var user = await userManager.FindByNameAsync(register.Name!);
        if (user is not null)
        {
            return BadRequest("User already registered");
        }

        user = await userManager.FindByEmailAsync(register.Email!);
        if (user is not null)
        {
            return BadRequest("Email already registered");
        }

        user = new IdentityUser(register.Name!) { Email = register.Email };

        var operationResult = await userManager.CreateAsync(user, register.Password!);

        return operationResult is not null && operationResult.Succeeded ? Ok() : StatusCode(500, operationResult?.Errors);
    }

    [HttpPost]
    [Route("login")]
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

        var getUserRole = await userManager.GetRolesAsync(user);

        var token = TokenService.GenerateToken(user.Id, user.UserName!, user.Email ?? user.UserName!, String.Join(",", getUserRole) );

        // await userManager.SetAuthenticationTokenAsync(user,"jwt","jwt",token);
        // await userManager.AddLoginAsync(user, new UserLoginInfo("jwt","jwt","jwt"));

        return Ok(token);
    }

    [HttpGet]
    [Authorize]
    [Route("info")]
    public async Task<IActionResult> Info()
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