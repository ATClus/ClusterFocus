using Microsoft.EntityFrameworkCore;
using Hub.Domain;

namespace Hub.Infrastructure
{
    public class HubDbContext : DbContext
    {
        public HubDbContext(DbContextOptions<HubDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tab> Tabs { get; set; }
        public DbSet<ApplicationOS> Applications { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tab>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(t => t.Url)
                      .IsRequired();
                entity.Property(t => t.Date)
                      .IsRequired();
            });

            modelBuilder.Entity<ApplicationOS>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Title)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(a => a.ProcessName)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(a => a.Date)
                      .IsRequired();
            });

            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.HasKey(te => te.Id);
                entity.Property(te => te.Date)
                      .IsRequired();
                entity.Property(te => te.Duration)
                      .IsRequired();
                entity.Property(te => te.CurrentSessionStart)
                      .IsRequired(false);

                entity.HasOne(te => te.Tab)
                      .WithMany(t => t.Sessions)
                      .HasForeignKey(te => te.TabId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false);

                entity.HasOne(te => te.Application)
                      .WithMany(a => a.Sessions)
                      .HasForeignKey(te => te.ApplicationId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false);
            });
        }
    }
}
