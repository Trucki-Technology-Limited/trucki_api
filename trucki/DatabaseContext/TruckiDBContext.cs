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
            modelBuilder.Entity<Driver>()
                .HasOne(d => d.Truck)
                .WithOne(t => t.Driver)
                .HasForeignKey<Driver>(d => d.TruckId);
            modelBuilder.Entity<Truck>() // Specify Truck as the principal
                .HasOne(t => t.Driver)
                .WithOne(d => d.Truck)
                .HasForeignKey<Driver>(d => d.TruckId);
        }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Routes> RoutesEnumerable { get; set; }
        public DbSet<TruckOwner> TruckOwners { get; set; }
        public DbSet<Officer> Officers { get; set; }
        public DbSet<Truck> Trucks { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<BankDetails> BankDetails { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<DriverDocument> DriverDocuments { get; set; }



    }
}
