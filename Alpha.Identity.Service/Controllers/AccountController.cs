using Alpha.Identity.Common.DTO;
using Alpha.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Alpha.Identity.Controllers;

[Route("/api/account")]
public class AccountController(UserManager<IdentityUser> userManager, ITokenService TokenService) : ControllerBase
{
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] AccountLogin login)
    {
        if (login?.Email is null)
        {
            return BadRequest("Invalid Email");
        }

        if (login?.Password is null)
        {
            return BadRequest("Invalid Password");
        }

        var user = await userManager.FindByNameAsync(login.Email);
        if (user is not null)
        {
            return BadRequest("User already registered");
        }

        var operationResult = await userManager.CreateAsync(new IdentityUser(login.Email), login.Password);

        return operationResult is not null && operationResult.Succeeded ? Ok() : StatusCode(500, operationResult?.Errors);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] AccountLogin login)
    {
        if (login?.Email is null)
        {
            return BadRequest("Invalid Email");
        }

        if (login?.Password is null)
        {
            return BadRequest("Invalid Password");
        }

        var user = await userManager.FindByNameAsync(login.Email);
        if (user is null)
        {
            return BadRequest("Invalid email/password");
        }

        bool checkUserPasswords = await userManager.CheckPasswordAsync(user, login.Password);
        if (!checkUserPasswords)
            return BadRequest("Invalid email/password");

        var getUserRole = await userManager.GetRolesAsync(user);

        var token = TokenService.GenerateToken(user.Id, user.UserName!, user.Email ?? user.UserName!, String.Join(",", getUserRole) );

        return Ok(token);
    }

}