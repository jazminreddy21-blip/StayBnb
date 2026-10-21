
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayBnb.Models;

namespace StayBnb.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {

        public DbSet<Property> Properties { get; set; }

        public DbSet<PropertyImage> PropertyImages { get; set; }

        public DbSet<HostApplication> HostApplications { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<CheckInProcess> CheckInProcesses { get; set; }

        public DbSet<GuestDocument> GuestDocuments { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // A message thats has a sender and a receiver.
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // A property that belongs to a host.
            builder.Entity<Property>()
                .HasOne(p => p.Host)
                .WithMany()
                .HasForeignKey(p => p.HostId)
                .OnDelete(DeleteBehavior.Restrict);

            // A booking that belongs to a guest.
            builder.Entity<Booking>()
                .HasOne(b => b.Guest)
                .WithMany()
                .HasForeignKey(b => b.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            // A host application that belongs to a user.
            builder.Entity<HostApplication>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // A guest document that belongs to a guest.
            builder.Entity<GuestDocument>()
                .HasOne(d => d.Guest)
                .WithMany()
                .HasForeignKey(d => d.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Keep activity logs if a user account is deleted.
            builder.Entity<ActivityLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure monetary values to use two decimal places.
            builder.Entity<Property>()
                .Property(p => p.PricePerNight)
                .HasPrecision(18, 2);

            builder.Entity<Property>()
                .Property(p => p.CleaningFee)
                .HasPrecision(18, 2);

            builder.Entity<Property>()
                .Property(p => p.ServiceFee)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasPrecision(18, 2);
        }
    }
}
    
