﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using memo.Models;

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

        // MODEL BUILDER
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            });

            // If this is missing, it gives exception:
            // The entity type 'IdentityUserLogin<string>' requires a primary key to be defined. If you intended to use a keyless entity type call 'HasNoKey()'
            base.OnModelCreating(modelBuilder);
        }
    }
}
