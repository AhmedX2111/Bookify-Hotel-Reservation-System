// Bookify-Hotel-Reservation-System-master/Bookify.Application/Interfaces/Data/IBookingRepository.cs
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
        Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId, CancellationToken cancellationToken = default);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, CancellationToken cancellationToken = default);

        // New: Get all bookings for admin, with optional filters
        Task<IEnumerable<Booking>> GetAllBookingsWithDetailsAsync(
            string? status = null,
            DateTime? checkInFrom = null,
            DateTime? checkInTo = null,
            CancellationToken cancellationToken = default);
    }
}
