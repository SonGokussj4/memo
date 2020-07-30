using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using memo.Models;

namespace memo.Data
{
    public partial class EvektorDbContext : DbContext
    {
        public EvektorDbContext()
        {
        }

        public EvektorDbContext(DbContextOptions<EvektorDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<tWorks> tWorks { get; set; }
        public virtual DbSet<cOrders> cOrders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<tWorks>(entity =>
            {
                entity.Property(e => e.EnterTime).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<cOrders>(entity =>
            {
                entity.Property(e => e.OrderCode).IsUnicode(false);

                entity.Property(e => e.OrderName).IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }


        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
