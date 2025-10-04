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
		// RESERVATION FUNCTIONALITY: Get user's bookings
		public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId, CancellationToken cancellationToken = default)
		{
			return await _dbContext.Bookings
				.Include(b => b.Room)
				.ThenInclude(r => r.RoomType)
				.Where(b => b.UserId == userId)
				.OrderByDescending(b => b.CreatedAt)
				.ToListAsync(cancellationToken);
		}
		// RESERVATION FUNCTIONALITY: Check room availability for specific dates
		public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, CancellationToken cancellationToken = default)
		{
			var conflictingBookings = await _dbContext.Bookings
				.Where(b => b.RoomId == roomId &&
						   (b.Status == "Confirmed" || b.Status == "Pending") &&
						   b.CheckInDate <= checkOutDate && b.CheckOutDate >= checkInDate)
				.ToListAsync(cancellationToken);

			return !conflictingBookings.Any();
		}

		// CONCURRENCY CONTROL: Get booking with lock for safe updates
		public async Task<Booking?> GetByIdWithLockAsync(int id, CancellationToken cancellationToken = default)
		{
			// Using raw SQL for row-level locking to prevent concurrent modifications
			return await _dbContext.Bookings
				.FromSqlInterpolated($@"SELECT * FROM Bookings WITH (UPDLOCK) WHERE Id = {id}")
				.Include(b => b.Room)
				.FirstOrDefaultAsync(cancellationToken);
		}

		// RESERVATION FUNCTIONALITY: Get bookings for a specific room and date range
		public async Task<IEnumerable<Booking>> GetBookingsForRoomAsync(int roomId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
		{
			return await _dbContext.Bookings
				.Where(b => b.RoomId == roomId &&
						   b.CheckInDate <= endDate &&
						   b.CheckOutDate >= startDate &&
						   (b.Status == "Confirmed" || b.Status == "Pending"))
				.OrderBy(b => b.CheckInDate)
				.ToListAsync(cancellationToken);
		}
	}
}
