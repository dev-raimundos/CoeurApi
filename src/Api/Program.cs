using CoeurApi.Api.Extensions;
using CoeurApi.Api.Pages;
using CoeurApi.Infrastructure;
using CoeurApi.Modules.Users.Infrastructure.Module;
using CoeurApi.Modules.Authentication.Infrastructure.Module;

namespace CoeurApi.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddSwaggerDocumentation();

        builder.Services.AddUsersModule();
        builder.Services.AddAuthModule(builder.Configuration);

        builder.Services.AddApiServices(builder.Configuration);

        var app = builder.Build();

        await app.MigrateDatabaseAsync();

        app.UseApiServices();
        app.UseSwaggerDocumentation();

        app.MapGet("/", (IWebHostEnvironment env) =>
            Results.Content(StatusPage.Render(env), "text/html"));

        app.MapControllers();

        await app.RunAsync();
    }
}
