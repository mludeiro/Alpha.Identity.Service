using System.Text;
using Alpha.Utils.Database;
using Alpha.Identity.Data;
using Alpha.Identity.DTO;
using Alpha.Identity.Model;
using Alpha.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Refit;
using Alpha.Utils.Consul;

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
        builder.Services.AddSingleton(jwtOptions);

        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Key!)),

            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        builder.Services.AddSingleton(tokenValidationParameters);

        builder.Services.AddScoped<IIdentityTokenService, IdentityTokenService>();
        builder.Services.AddHostedService<DbMigrationBackgroundService<DataContext>>();

        builder.Services.AddDbContext<DataContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")!));

        builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = tokenValidationParameters;
            });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(PolicyClaim.identityUserMe, authBuilder => { authBuilder.RequireClaim(PolicyClaim.identityUserMe); });
        
        builder.Services.AddIdentity<AlphaUser,IdentityRole>()
           .AddEntityFrameworkStores<DataContext>()
           .AddDefaultTokenProviders();
        
        builder.Services.ConsulServicesConfig(builder.Configuration.GetSection("Consul").Get<ConsulConfig>()!);

        builder.Services.AddScoped<ConsulRegistryHandler>();
        
        builder.Services.AddRefitClient<IRestTokenService>( )
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("http://token.service:8080"))
            .AddHttpMessageHandler<ConsulRegistryHandler>();
            

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

// app.Start();

// var server = app.Services.GetService<IServer>();
// var addressFeature = server.Features.Get<IServerAddressesFeature>();

// foreach (var address in addressFeature.Addresses)
// {
//     Console.WriteLine("Kestrel is listening on address: " + address);
// }

// app.WaitForShutdown();
    }



}

