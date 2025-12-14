using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FPTMart.DAL.Data;

/// <summary>
/// Design-time factory for EF Core migrations
/// Allows running migrations from FPTMart.DAL project
/// </summary>
public class FPTMartDbContextFactory : IDesignTimeDbContextFactory<FPTMartDbContext>
{
    public FPTMartDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FPTMartDbContext>();
        
        // Default connection string for migrations
        // Change this to match your SQL Server instance
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=FPTMartDb;Trusted_Connection=True;TrustServerCertificate=True;"
        );

        return new FPTMartDbContext(optionsBuilder.Options);
    }
}
