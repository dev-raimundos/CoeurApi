using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using CoeurApi.Api.Extensions;
using CoeurApi.Api.Pages;
using CoeurApi.Infrastructure;
using CoeurApi.Infrastructure.Persistence;
using CoeurApi.Modules.Users.Infrastructure.Module;
using CoeurApi.Modules.Authentication.Infrastructure.Module;

namespace CoeurApi.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Coeur API", Version = "v1" });

            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Informe apenas o token JWT retornado pelo login (sem o prefixo \"Bearer\")."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });

        builder.Services.AddUsersModule();
        builder.Services.AddAuthModule(builder.Configuration);

        builder.Services.AddApiServices(builder.Configuration);

        var app = builder.Build();

        // Desligável via config (Database:AutoMigrate=false) pra ambientes com múltiplas
        // réplicas, onde migration deve rodar como step separado do deploy, não no startup.
        if (app.Configuration.GetValue("Database:AutoMigrate", true))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        app.UseApiServices();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Coeur API v1");
            });
        }

        app.MapGet("/", (IWebHostEnvironment env) =>
            Results.Content(StatusPage.Render(env), "text/html"));

        app.MapControllers();

        await app.RunAsync();
    }
}
