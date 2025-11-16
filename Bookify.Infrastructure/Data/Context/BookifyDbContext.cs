using Bookify.Application.Business.Interfaces.Data;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data.Data.Context
{
    public class BookifyDbContext : IdentityDbContext<User>, IBookfiyDbContext
    {
        public BookifyDbContext(DbContextOptions<BookifyDbContext> options) : base(options) { }

        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<RoomType> RoomTypes => Set<RoomType>();
        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all IEntityTypeConfiguration<T> automatically
            builder.ApplyConfigurationsFromAssembly(typeof(BookifyDbContext).Assembly);

            // Global precision configs (optional, can also move to configs)
            builder.Entity<RoomType>()
                .Property(rt => rt.PricePerNight)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.TotalCost)
                .HasPrecision(18, 2);

            // New: Convert BookingStatus enum to string for DB storage
            builder.Entity<Booking>()
                .Property(b => b.Status)
                .HasConversion<string>();
        }

    }

}
