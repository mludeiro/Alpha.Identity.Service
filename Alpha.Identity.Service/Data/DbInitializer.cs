using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alpha.Identity.Data;


public class DbInitializer(IServiceProvider serviceProvider) :  BackgroundService
{
    private static readonly string[] roles = 
    [
        "Identity.User.Read",
        "Identity.User.Write"
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(2000, stoppingToken);

        var scope = serviceProvider.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        dataContext.Database.Migrate();

        foreach( var role in roles )
        {
            var obj = await roleManager.FindByNameAsync(role);
            if( obj is null )
                await roleManager.CreateAsync(new IdentityRole(){ Name = role });
        }
    }
}