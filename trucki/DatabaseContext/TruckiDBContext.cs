using System;
using System.Text.Json;
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
            // New CargoOrders configurations
            modelBuilder.Entity<CargoOrders>(builder =>
            {
                builder.HasKey(x => x.Id);

                // Configure CargoOwner relationship
                builder.HasOne(co => co.CargoOwner)
                    .WithMany(o => o.Orders)
                    .HasForeignKey(co => co.CargoOwnerId);

                // Configure Bids collection relationship
                builder.HasMany(co => co.Bids)
                    .WithOne(b => b.Order)
                    .HasForeignKey(b => b.OrderId);

                // Configure AcceptedBid relationship
                builder.HasOne(co => co.AcceptedBid)
                    .WithOne()
                    .HasForeignKey<CargoOrders>("AcceptedBidId");

                // Configure Documents and DeliveryDocuments as comma-separated strings
                builder.Property(co => co.Documents)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

                builder.Property(co => co.DeliveryDocuments)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            });

            // New Bid configurations
            modelBuilder.Entity<Bid>(builder =>
            {
                builder.HasKey(x => x.Id);

                // Configure Order relationship
                builder.HasOne(b => b.Order)
                    .WithMany(o => o.Bids)
                    .HasForeignKey(b => b.OrderId);

                // Configure Truck relationship
                builder.HasOne(b => b.Truck)
                    .WithMany()
                    .HasForeignKey(b => b.TruckId);
            });
            modelBuilder.Entity<ChatMessage>(entity =>
          {
              entity.HasKey(e => e.Id);
              entity.Property(e => e.OrderId).IsRequired();
              entity.Property(e => e.SenderId).IsRequired();
              entity.Property(e => e.RecipientId).IsRequired();
              entity.Property(e => e.Text).IsRequired();
              entity.Property(e => e.Timestamp).IsRequired();
              entity.Property(e => e.IsRead).HasDefaultValue(false);

              // Store ImageUrls as JSON
              entity.Property(e => e.ImageUrls).HasConversion(
                  v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                  v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());

              // Relationship with CargoOrders
              entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId);
          });
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
        public DbSet<CargoOwner> CargoOwners { get; set; }
        public DbSet<CargoOrders> CargoOrders { get; set; }
        public DbSet<DeliveryLocationUpdate> deliveryLocationUpdates { get; set; }
        public DbSet<DriverBankAccount> driverBankAccounts { get; set; }
        public DbSet<TermsAcceptanceRecord> TermsAcceptanceRecords { get; set; }
        public DbSet<TermsAndConditions> TermsAndConditions { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<DeviceToken> DeviceTokens { get; set; }

    }
}
