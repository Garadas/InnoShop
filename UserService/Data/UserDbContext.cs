using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(191).IsRequired();
                entity.Property(u => u.Name).HasMaxLength(100).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.EmailConfirmationToken).HasMaxLength(200);
                entity.Property(u => u.ResetPasswordToken).HasMaxLength(200);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
