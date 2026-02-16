using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Data
{
    public class EventReservationDbcontext(DbContextOptions<EventReservationDbcontext>options) : DbContext(options)
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Venue> Venues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>(e =>
            {
                e.Property(t => t.Price)
                    .HasColumnType("decimal(18,2)");
                e.HasOne(t => t.Seat)
                    .WithMany(s => s.Tickets)
                    .HasForeignKey(t => t.SeatId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(t => t.Event)
                    .WithMany(s => s.Tickets)
                    .HasForeignKey(t => t.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(t => new { t.EventId, t.SeatId }).IsUnique();
                e.Property(t => t.TicketCode).HasMaxLength(50);
            });
            modelBuilder.Entity<Event>(e =>
            {
                e.HasOne(t => t.Venue)
                    .WithMany(t => t.Events)
                    .HasForeignKey(t => t.VenueId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(v => v.Name).HasMaxLength(150).IsRequired();
                e.Property(v => v.BasePrice).HasColumnType("decimal(4,1)").IsRequired();

            });
            modelBuilder.Entity<Seat>(e =>
            {
                e.HasOne(t => t.Venue)
                    .WithMany(t => t.Seats)
                    .HasForeignKey(t => t.VenueId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(s => s.Class).HasMaxLength(20); 
            });
            modelBuilder.Entity<Venue>(e =>
            {
                e.Property(v => v.Name).HasMaxLength(100).IsRequired();
                e.Property(v => v.Location).HasMaxLength(200);
            });
        }
    }
}
