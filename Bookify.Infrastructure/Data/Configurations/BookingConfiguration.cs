using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data.Data.Configurations
{
	public class BookingConfiguration : IEntityTypeConfiguration<Booking>
	{
		public void Configure(EntityTypeBuilder<Booking> builder)
		{
			builder.HasOne(b => b.User)
			   .WithMany(u => u.Bookings)
			   .HasForeignKey(b => b.UserId);

			builder.HasOne(b => b.Room)
				.WithMany(r => r.Bookings)
				.HasForeignKey(b => b.RoomId);
		}
	}
}
