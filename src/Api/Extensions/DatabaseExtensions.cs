using Microsoft.EntityFrameworkCore;
using CoeurApi.Infrastructure.Persistence;

namespace CoeurApi.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        // Desligável via config (Database:AutoMigrate=false) pra ambientes com múltiplas
        // réplicas, onde migration deve rodar como step separado do deploy, não no startup.
        if (!app.Configuration.GetValue("Database:AutoMigrate", true))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
