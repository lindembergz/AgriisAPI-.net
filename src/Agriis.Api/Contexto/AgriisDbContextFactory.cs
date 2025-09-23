using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Agriis.Api.Contexto;

public class AgriisDbContextFactory : IDesignTimeDbContextFactory<AgriisDbContext>
{
    public AgriisDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AgriisDbContext>();
        
        // Connection string para design-time (migrations)
        var connectionString = "Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123;Port=5432;SSL Mode=Disable";
        
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            
            // TODO: Habilitar extensão PostGIS para suporte geográfico quando disponível
            // options.UseNetTopologySuite();
        });

        return new AgriisDbContext(optionsBuilder.Options);
    }
}