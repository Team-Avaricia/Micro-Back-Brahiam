using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<FinancialRule> FinancialRules { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<TelegramLinkCode> TelegramLinkCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CurrentBalance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PasswordHash).HasMaxLength(500);
                entity.Property(e => e.TelegramUsername).HasMaxLength(100);
                
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                entity.HasIndex(e => e.TelegramId).IsUnique().HasFilter("\"TelegramId\" IS NOT NULL");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.Source).HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Date);
            });

            modelBuilder.Entity<FinancialRule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.AmountLimit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.Period).HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.FinancialRules)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.IsActive });
            });

            modelBuilder.Entity<RecurringTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.Frequency).HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.IsActive, e.NextExecutionDate });
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedByIp).HasMaxLength(50);
                entity.Property(e => e.RevokedByIp).HasMaxLength(50);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.IsRevoked, e.ExpiresAt });
            });

            modelBuilder.Entity<TelegramLinkCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.IsUsed });
            });
        }
    }
}
