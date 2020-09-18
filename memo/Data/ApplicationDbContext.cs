using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using memo.Models;
using memo.ViewModels;

namespace memo.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // TABLES
        // dotnet ef dbcontext scaffold "Server=ar-nexus,1433;Initial catalog=MemoDB;User ID=SA;Password=Pa55w0rd2019" Microsoft.EntityFrameworkCore.SqlServer -o Models -t Contact --context-dir Data --data-annotations
        public virtual DbSet<Contact> Contact { get; set; }
        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<Currency> Currency { get; set; }
        public virtual DbSet<Offer> Offer { get; set; }
        public virtual DbSet<OfferStatus> OfferStatus { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<Invoice> Invoice { get; set; }
        public virtual DbSet<OtherCost> OtherCost { get; set; }

        // PROCEDURES
        public virtual DbSet<SumMinutesSP> SumMinutesSP { get; set; }
        // public virtual DbSet<DashboardVM> DashboardVM { get; set; }

        // EVE
        public virtual DbSet<tWorks> tWorks { get; set; }
        public virtual DbSet<cOrders> cOrders { get; set; }

        // // ViewModels
        // public DbSet<CreateOfferViewModel> CreateOfferVM { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .EnableSensitiveDataLogging();
        }

        // MODEL BUILDER
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SumMinutesSP>(entity => {
                entity.HasNoKey();
            });

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Offer>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Offers)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK__Offer__CompanyId__30C33EC3");

                entity.HasOne(d => d.Contact)
                    .WithMany(p => p.Offers)
                    .HasForeignKey(d => d.ContactId)
                    .HasConstraintName("FK__Offer__ContactId__2FCF1A8A");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Offer)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("FK__Offer__CurrencyI__3493CFA7");

                entity.HasOne(d => d.OfferStatus)
                    .WithMany(p => p.Offer)
                    .HasForeignKey(d => d.OfferStatusId)
                    .HasConstraintName("FK__Offer__OfferStatusId__31B762FC");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.OrderName)
                    .HasName("UQ_OrderName")
                    .IsUnique();

                entity.Property(e => e.CreateDate)
                    .HasDefaultValueSql("(getdate())");
            });

            // If this is missing, it gives exception:
            // The entity type 'IdentityUserLogin<string>' requires a primary key to be defined. If you intended to use a keyless entity type call 'HasNoKey()'
            base.OnModelCreating(modelBuilder);
        }
    }
}
