using Microsoft.EntityFrameworkCore;

namespace RabbitMQ_Watermark.Web.Models
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; } = null!;
    }
}
