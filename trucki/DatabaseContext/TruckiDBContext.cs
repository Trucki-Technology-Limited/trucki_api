﻿using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using trucki.Entities;

namespace trucki.DatabaseContext
{
    public class TruckiDBContext : IdentityDbContext<User>
    {

        public TruckiDBContext(DbContextOptions<TruckiDBContext> options) : base(options)
        {

        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Routes> RoutesEnumerable { get; set; }
    }
}
