using Alpha.Identity.Data;
using Alpha.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alpha.Identity;

internal class Program
{
    private static void Main(string[] args)
    {
        Run(Build(args));
    }

    private static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddHealthChecks();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<ITokenService, TokenService>();

        builder.Services.AddDbContext<DataContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")!));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAuthorization();
        builder.Services.AddIdentityApiEndpoints<IdentityUser>()
            .AddRoles<IdentityRole>()
           .AddEntityFrameworkStores<DataContext>()
           ;

        var app = builder.Build();
        return app;
    }

    private static void Run(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

//        app.MapIdentityApi<IdentityUser>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        MigrateDb(app);

        app.Run();
    }


    private static void MigrateDb(WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var dc = scope.ServiceProvider.GetRequiredService<DataContext>();
        dc.Database.Migrate();
    }
}