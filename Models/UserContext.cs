using lab3app.Models;
using Microsoft.EntityFrameworkCore;

namespace lab3app.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        //DbSet for the Users table
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Configure the Users table
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.UserID);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.UserPassword)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.DateCreated)
                    .HasDefaultValueSql("GETDATE()");
            });
        }
    }
}
