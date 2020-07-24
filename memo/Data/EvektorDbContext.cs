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

        public virtual DbSet<TWorks> TWorks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TWorks>(entity =>
            {
                entity.Property(e => e.EnterTime).HasDefaultValueSql("(getdate())");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
