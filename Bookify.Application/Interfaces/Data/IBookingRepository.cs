using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Data
{
	public interface IBookingRepository : IRepository<Booking>
	{
		// Reservation functionality
		Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId, CancellationToken cancellationToken = default);
		Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, CancellationToken cancellationToken = default);
		Task<IEnumerable<Booking>> GetBookingsForRoomAsync(int roomId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

		// Admin functionality

		// Concurrency control
		Task<Booking?> GetByIdWithLockAsync(int id, CancellationToken cancellationToken = default);
	}
}
