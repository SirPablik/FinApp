using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FinPlan.Web.Models;

namespace FinPlan.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Таблицы базы данных
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Investment> Investments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Настройка поля FullName для ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FullName)
                    .IsRequired()  // Обязательно в БД
                    .HasMaxLength(200);  // Максимальная длина
            });
            builder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Transactions)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Budget>(entity =>
            {
                entity.HasOne(b => b.User)
                      .WithMany(u => u.Budgets)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                
                entity.Property(b => b.UserId)
                      .IsRequired(false); 
            });
            builder.Entity<Investment>(entity =>
            {
                entity.HasOne(i => i.User)
                      .WithMany(u => u.Investments)
                      .HasForeignKey(i => i.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(i => i.UserId)
                      .IsRequired(false);  
            });
        }
    }
}