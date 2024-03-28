using Alpha.Identity.Data;
using Alpha.Identity.Model;
using Alpha.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Refit;
using Alpha.Common.Consul;
using Alpha.Common.Database;
using Alpha.Common.TokenService;
using Alpha.Common.Security;
using Dapr.Client;

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
        builder.Services.AddMemoryCache();


        builder.Services.AddSwaggerGen(o =>
        {
            o.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            o.OperationFilter<SecurityRequirementsOperationFilter>();
        });
        
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
        builder.Services.AddAlphaAuthentication(jwtOptions);

        builder.Services.AddScoped<IIdentityTokenService, IdentityTokenService>();
        builder.Services.AddHostedService<DbMigrationBackgroundService<DataContext>>();

        builder.Services.AddDbContext<DataContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")!));

        builder.Services.AddAuthorizationBuilder().AddAlphaAuthorizationPolicies();
        
        builder.Services.AddIdentity<AlphaUser,IdentityRole>()
           .AddEntityFrameworkStores<DataContext>()
           .AddDefaultTokenProviders();
        
        // builder.Services.ConsulServicesConfig(builder.Configuration.GetSection("Consul").Get<ConsulConfig>()!);

        // builder.Services.AddScoped<ConsulRegistryHandler>();
        
        // builder.Services.AddRefitClient<ITokenService>()
        //     .ConfigureHttpClient(client => client.BaseAddress = new Uri("http://token.service:8080"))
        //     .AddHttpMessageHandler<ConsulRegistryHandler>();

        builder.Services.AddRefitClient<ITokenService>()
            .ConfigureHttpClient(options => { options.BaseAddress = new Uri($"http://token"); })
            .AddHttpMessageHandler(_ => new InvocationHandler());


        var app = builder.Build();
        
        return app;
    }

    private static void Run(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service"));
        }

        // app.MapIdentityApi<IdentityUser>();
        // app.UseHttpsRedirection();
        // app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }



}

