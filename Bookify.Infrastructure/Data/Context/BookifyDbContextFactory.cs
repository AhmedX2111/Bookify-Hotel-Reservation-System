using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bookify.Infrastructure.Data.Data.Context
{
    // هذه الفئة ضرورية لـ Entity Framework Core لإنشاء DbContext
    // عند تشغيل أوامر migrations و database update
    public class BookifyDbContextFactory : IDesignTimeDbContextFactory<BookifyDbContext>
    {
        public BookifyDbContext CreateDbContext(string[] args)
        {
            // يجب أن تكون سلسلة الاتصال هنا هي نفسها التي في appsettings.json
            const string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=BookifyDb;Trusted_Connection=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<BookifyDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new BookifyDbContext(optionsBuilder.Options);
        }
    }
}