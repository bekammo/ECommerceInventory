using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerceInventory.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=ECommerceInventory;Trusted_Connection=true;TrustServerCertificate=True");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
