using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Data
{
	public interface IBookfiyDbContext
	{
		DbSet<Room> Rooms { get; }
		DbSet<RoomType> RoomTypes { get; }
		DbSet<Booking> Bookings { get; }
		DbSet<User> Users { get; }

		Task<int> SaveChangesAsync(CancellationToken cancellationToken);
	}
}
