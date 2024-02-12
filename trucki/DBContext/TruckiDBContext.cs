using Microsoft.EntityFrameworkCore;
using trucki.Models;

namespace trucki.DBContext
{
    public class TruckiDBContext : DbContext
    {

        public TruckiDBContext(DbContextOptions<TruckiDBContext> options) : base(options)
        {

        }

        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Location> Locations { get; set; }
    }
}
