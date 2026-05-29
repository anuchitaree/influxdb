using Microsoft.EntityFrameworkCore;
namespace messagequque.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MessageEntity> Messages => Set<MessageEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageEntity>()
                .HasIndex(x => x.MessageId)
                .IsUnique();
        }
    }
}
