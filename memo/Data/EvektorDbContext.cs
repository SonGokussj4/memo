using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using memo.Models;
using memo.ViewModels;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq;

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
        public virtual DbSet<tUsers> tUsers { get; set; }

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

            modelBuilder.Entity<tUsers>(entity =>
            {
                entity.ToTable("tUsers");

                entity.HasIndex(e => e.IdtUsersCategory)
                    .HasName("IX_tUsers_IDtUsersCategory");

                entity.HasIndex(e => e.IdworkTime)
                    .HasName("tUsers_IDWorktime");

                entity.HasIndex(e => new { e.Id, e.IntAccType, e.Del, e.LastName, e.FirstName, e.FirstTitle, e.TxAccount })
                    .HasName("IX_Users_AccType_Del_ID");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.BirthNum)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.BranchId)
                    .HasColumnName("BranchID")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Del).HasDefaultValueSql("(0)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("First_Name")
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.FirstTitle)
                    .IsRequired()
                    .HasColumnName("First_Title")
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.FormatedName)
                    .HasMaxLength(173)
                    .IsUnicode(false)
                    .HasComputedColumnSql("(rtrim(ltrim((isnull([Last_Name],'') + isnull((' ' + [First_Name]),'') + isnull((' ' + [First_Title]),'') + isnull((' ' + [Last_title]),'')))))");

                entity.Property(e => e.Idfirm).HasColumnName("IDFirm");

                entity.Property(e => e.IdtUsersCategory)
                    .HasColumnName("IDtUsers_Category")
                    .HasDefaultValueSql("(1)");

                entity.Property(e => e.IduserCreate)
                    .HasColumnName("IDUserCreate")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.IduserModify)
                    .HasColumnName("IDUserModify")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.IdworkTime)
                    .HasColumnName("IDWorkTime")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.IntAccType).HasColumnName("intAccType");

                entity.Property(e => e.JobDateFrom).HasColumnType("datetime");

                entity.Property(e => e.JobDateTo).HasColumnType("datetime");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("Last_Name")
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.LastTitle)
                    .IsRequired()
                    .HasColumnName("Last_Title")
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Mobil)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ModifyTime).HasColumnType("datetime");

                entity.Property(e => e.Note)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.OperatorId)
                    .HasColumnName("OperatorID")
                    .HasDefaultValueSql("(0)");

                entity.Property(e => e.OverLimit).HasDefaultValueSql("(416)");

                entity.Property(e => e.PersNum)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Pozn)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Sex).HasDefaultValueSql("(0)");

                entity.Property(e => e.Telefon)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TxAccount)
                    .IsRequired()
                    .HasColumnName("txAccount")
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Urlpict)
                    .IsRequired()
                    .HasColumnName("URLPict")
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.WorkMask).HasDefaultValueSql("(0)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

    public static class DbFunctionExtensions
    {
        public static int? DatePart(string type, DateTime? date) => throw new Exception();

        public static void ConfigureDbFunctions(this ModelBuilder modelBuilder)
        {
            var mi = typeof(DbFunctionExtensions).GetMethod(nameof(DatePart));

            modelBuilder.HasDbFunction(mi, b => b.HasTranslation(e =>
            {
                var ea = e.ToArray();
                var args = new[]
                {
                    new SqlFragmentExpression((ea[0] as SqlConstantExpression).Value.ToString()),
                    ea[1]
                };
                return SqlFunctionExpression.Create(nameof(DatePart), args, typeof(int?), null);
            }));
        }
    }
}
