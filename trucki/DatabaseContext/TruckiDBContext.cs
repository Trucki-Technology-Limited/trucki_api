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
            modelBuilder.Entity<DatabaseNotification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsRead).HasDefaultValue(false);

                // Relationship with User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId);
            });
            // Configure CargoOwnerWallet
            modelBuilder.Entity<CargoOwnerWallet>()
                .HasOne(w => w.CargoOwner)
                .WithOne()
                .HasForeignKey<CargoOwnerWallet>(w => w.CargoOwnerId);

            // Configure WalletTransaction
            modelBuilder.Entity<WalletTransaction>()
                .HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(t => t.RelatedOrder)
                .WithMany()
                .HasForeignKey(t => t.RelatedOrderId)
                .IsRequired(false);  // Optional relationship

            // Configure CargoOrders updates
            modelBuilder.Entity<CargoOrders>()
                .Property(o => o.WalletPaymentAmount)
                .IsRequired(false);

            modelBuilder.Entity<CargoOrders>()
                .Property(o => o.StripePaymentAmount)
                .IsRequired(false);
            // Add configurations for new entities
            modelBuilder.Entity<DriverWallet>()
                .HasOne(w => w.Driver)
                .WithOne()
                .HasForeignKey<DriverWallet>(w => w.DriverId);

            modelBuilder.Entity<DriverWalletTransaction>()
                .HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId);

            modelBuilder.Entity<DriverWithdrawalSchedule>()
                .HasMany(s => s.Transactions)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DriverRating>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => x.CargoOrderId).IsUnique();

                builder.HasOne(r => r.CargoOrder)
                    .WithOne()
                    .HasForeignKey<DriverRating>(r => r.CargoOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(r => r.Driver)
                    .WithMany()
                    .HasForeignKey(r => r.DriverId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(r => r.CargoOwner)
                    .WithMany()
                    .HasForeignKey(r => r.CargoOwnerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AccountDeletionRequest
            modelBuilder.Entity<AccountDeletionRequest>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure DriverDispatcherCommission entity relationships
            modelBuilder.Entity<DriverDispatcherCommission>(builder =>
            {
                builder.HasKey(x => x.Id);

                // Configure Driver relationship
                builder.HasOne(dc => dc.Driver)
                    .WithMany(d => d.CommissionStructures)
                    .HasForeignKey(dc => dc.DriverId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure Dispatcher (TruckOwner) relationship
                builder.HasOne(dc => dc.Dispatcher)
                    .WithMany(to => to.DispatcherCommissions)
                    .HasForeignKey(dc => dc.DispatcherId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add unique index for active commissions (only one active commission per driver-dispatcher pair)
                builder.HasIndex(dc => new { dc.DriverId, dc.DispatcherId, dc.IsActive })
                    .HasFilter("IsActive = true")
                    .IsUnique()
                    .HasDatabaseName("IX_DriverDispatcherCommission_ActiveUnique");
            });

            // Update Bid entity configuration for dispatcher functionality
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

                // Configure SubmittedByDispatcher relationship (optional)
                builder.HasOne(b => b.SubmittedByDispatcher)
                    .WithMany()
                    .HasForeignKey(b => b.SubmittedByDispatcherId)
                    .IsRequired(false);
            });

            // Configure Driver entity relationships for dispatcher management
            modelBuilder.Entity<Driver>(builder =>
            {
                // Configure ManagedByDispatcher relationship
                builder.HasOne(d => d.ManagedByDispatcher)
                    .WithMany(to => to.drivers)
                    .HasForeignKey(d => d.ManagedByDispatcherId)
                    .IsRequired(false);
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
        public DbSet<DatabaseNotification> Notifications { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<CargoOwnerWallet> CargoOwnerWallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<DriverWallet> DriverWallets { get; set; }
        public DbSet<DriverWalletTransaction> DriverWalletTransactions { get; set; }
        public DbSet<DriverWithdrawalSchedule> DriverWithdrawalSchedules { get; set; }
        public DbSet<OrderCancellation> OrderCancellations { get; set; }
        public DbSet<DriverPayout> DriverPayouts { get; set; }
        public DbSet<DriverRating> DriverRatings { get; set; }
        public DbSet<AccountDeletionRequest> AccountDeletionRequests { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<DriverDispatcherCommission> DriverDispatcherCommissions { get; set; }
        public DbSet<OnboardingReminderTracking> OnboardingReminderTrackings { get; set; }



    }
}
