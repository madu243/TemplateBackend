using Microsoft.EntityFrameworkCore;
using TemplateBackend.Models;

namespace TemplateBackend.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
        { }
             public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── User Configuration ────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.HasIndex(u => u.Username)
                      .IsUnique();

                entity.Property(u => u.Role)
                      .HasDefaultValue("User");

                entity.Property(u => u.IsActive)
                      .HasDefaultValue(true);

                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(u => u.UpdatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // ── TaskItem Configuration ────────────────────
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.Property(t => t.Status)
                      .HasDefaultValue("In Progress");

                entity.Property(t => t.Priority)
                      .HasDefaultValue("Medium");

                entity.Property(t => t.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(t => t.UpdatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // One user has many tasks
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Tasks)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Index for faster queries
                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => t.Priority);
            });
        }
    }
}
