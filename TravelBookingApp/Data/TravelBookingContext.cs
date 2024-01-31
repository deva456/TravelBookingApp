using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelBooking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Data
{
    public class TravelBookingContext : IdentityDbContext
    {
        public TravelBookingContext(DbContextOptions<TravelBookingContext> options)
            : base(options)
        {
        }

        public DbSet<Airport> Airports { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<PlannedFlight> PlannedFlights { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<PurchasedFlight> PurchasedFlights { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //  optionsBuilder.UseSqlite(@"Data source=TravelBooking.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            base.OnModelCreating(modelBuilder);
            //  modelBuilder.Entity<FlightToAirport>()
            // .HasKey(f => new {f.AirportId,f.FlightId});
        }
    }
}