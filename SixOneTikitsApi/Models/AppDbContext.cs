using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using SixOneTikitsApi.Migrations;

namespace PianoBarApi.Models
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext() : base("DefaultConnection") { }

        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ResetRequest> ResetRequests { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketSale> TicketSales { get; set; }
        public DbSet<CancelledTicketSale> CancelledTicketSales { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Configuration.LazyLoadingEnabled = true;
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Configuration>());
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity)
                .OfType<IAuditable>())
            {
                entry.CreatedAt = DateTime.UtcNow;
                entry.ModifiedAt = DateTime.UtcNow;
            }

            foreach (var entry in ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified)
                .Select(x => x.Entity)
                .OfType<IAuditable>())
            {
                entry.ModifiedAt = DateTime.UtcNow;
            }
            return base.SaveChanges();
        }
    }
}