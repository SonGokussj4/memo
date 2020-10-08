using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using memo.Models;
using memo.ViewModels;

namespace memo.Data
{
    public class LoginDbContext : IdentityDbContext
    {
        public LoginDbContext(DbContextOptions<LoginDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        // MODEL BUILDER
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // If this is missing, it gives exception:
            // The entity type 'IdentityUserLogin<string>' requires a primary key to be defined. If you intended to use a keyless entity type call 'HasNoKey()'
            base.OnModelCreating(modelBuilder);
        }
    }
}
