using Bookify.Application.Business.Interfaces.Data;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Infrastructure.Data.Data.Repositories
{
	public class BookingRepository : Repository<Booking>, IBookingRepository
	{
		public BookingRepository(BookifyDbContext dbContext) : base(dbContext)
		{
		}

		public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId, CancellationToken cancellationToken = default)
		{
			return await _dbContext.Bookings
				.Include(b => b.Room)
				.ThenInclude(r => r.RoomType)
				.Where(b => b.UserId == userId)
				.OrderByDescending(b => b.CreatedAt)
				.ToListAsync(cancellationToken);
		}

		public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, CancellationToken cancellationToken = default)
		{
			var conflictingBookings = await _dbContext.Bookings
				.Where(b => b.RoomId == roomId &&
						   (b.Status == "Confirmed" || b.Status == "Pending") &&
						   b.CheckInDate <= checkOutDate && b.CheckOutDate >= checkInDate)
				.ToListAsync(cancellationToken);

			return !conflictingBookings.Any();
		}
	}
}
