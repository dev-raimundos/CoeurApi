using Microsoft.EntityFrameworkCore;
using CoeurApi.Modules.Users.Infrastructure.Persistence.Configurations;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.Modules.Users.Domain;

namespace CoeurApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}