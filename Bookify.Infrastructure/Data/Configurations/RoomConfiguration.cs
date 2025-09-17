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
	public class RoomConfiguration : IEntityTypeConfiguration<Room>
	{
		public void Configure(EntityTypeBuilder<Room> builder)
		{
			builder.Property(r => r.RoomNumber)
				.HasMaxLength(10)
				.IsRequired();

			builder.HasOne(r => r.RoomType)
				.WithMany(rt => rt.Rooms)
				.HasForeignKey(r => r.RoomTypeId);
		}
	}
}
