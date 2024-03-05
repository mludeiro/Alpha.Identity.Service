using System.Security.Claims;
using Alpha.Identity.Model;
using Microsoft.AspNetCore.Identity;

namespace Alpha.Identity.Data;


public class DbInitializer(IServiceProvider serviceProvider) :  BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);

        var scope = serviceProvider.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AlphaUser>>();

        var res = await roleManager.CreateAsync(new IdentityRole() { Name = "role1" });

        var res2 = await roleManager.CreateAsync(new IdentityRole() { Name = "role2" });
        var yo = await userManager.FindByNameAsync("mludeiro");

        var group1 = await roleManager.FindByNameAsync("role1");
        var group2 = await roleManager.FindByNameAsync("role2");

        await roleManager.AddClaimAsync(group1!, new Claim("Weather.Weather.Read", "true"));
        await roleManager.AddClaimAsync(group1!, new Claim("Identity.User.Read", "true"));

        await roleManager.AddClaimAsync(group2!, new Claim("Weather.Weather.Read", "true"));
        await roleManager.AddClaimAsync(group1!, new Claim("Other.Service.Read", "true"));

        await userManager.AddToRoleAsync(yo!, "role1");
        await userManager.AddToRoleAsync(yo!, "role2");
    }
}