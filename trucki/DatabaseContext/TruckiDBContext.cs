using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using trucki.Models;

namespace trucki.DBContext
{
    public class TruckiDBContext : IdentityDbContext<User>
    {

        public TruckiDBContext(DbContextOptions<TruckiDBContext> options) : base(options)
        {

        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "admin" },
                new ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "manager" },
                new ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "driver" },
                new ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "cargo owner" },
                new ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "transporter" },
                new ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "finance manager" },
                new ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "hr" }
            );
            modelBuilder.Entity<ApplicationPermission>().HasData(
                new ApplicationPermission { Id = Guid.NewGuid().ToString(), Name = "CreateManager" },
                new ApplicationPermission { Id = Guid.NewGuid().ToString(), Name = "EditManager" },
                new ApplicationPermission { Id = Guid.NewGuid().ToString(), Name = "ViewManager" },
                new ApplicationPermission { Id = Guid.NewGuid().ToString(), Name = "DeleteManager" },
                new ApplicationPermission { Id = Guid.NewGuid().ToString(), Name = "CreateDriver" },
                new ApplicationPermission { Id = Guid.NewGuid().ToString(), Name = "EditDriver" },
                new ApplicationPermission { Id = Guid.NewGuid().ToString(), Name = "ViewDriver" },
                new ApplicationPermission { Id = Guid.NewGuid().ToString(), Name = "DeleteDriver" }
            );

        }
        public DbSet<ApplicationRole> Roles { get; set; }
        public DbSet<ApplicationPermission> Permissions { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Location> Locations { get; set; }
    }
}
