using Microsoft.EntityFrameworkCore;
using Newsletter_Backend_Function.Models;

namespace Newsletter_Backend_Function.Data
{
    public class NewsletterDbContext : DbContext
    {
        public NewsletterDbContext(DbContextOptions<NewsletterDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subscriber> Subscribers { get; set; }
    }
}