// Bookify-Hotel-Reservation-System-master/Bookify.Application/Interfaces/Services/IBookingService.cs
using Bookify.Application.Business.Dtos.Bookings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Services
{
    public interface IBookingService
    {
        // Reservation Cart Functionality
        Task<bool> AddToReservationCartAsync(string userId, BookingCreateDto bookingDetails, CancellationToken cancellationToken = default);
        Task<List<BookingCreateDto>> GetReservationCartAsync(string userId, CancellationToken cancellationToken = default);
        Task<bool> RemoveFromReservationCartAsync(string userId, int roomId, CancellationToken cancellationToken = default);
        Task ClearReservationCartAsync(string userId, CancellationToken cancellationToken = default);

        // Admin Booking Management
        Task<IEnumerable<BookingDto>> GetAllBookingsAdminAsync(
            string? status = null,
            DateTime? checkInFrom = null,
            DateTime? checkInTo = null,
            CancellationToken cancellationToken = default);
        Task<BookingDto?> GetBookingByIdAdminAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UpdateBookingStatusAdminAsync(int bookingId, string newStatus, CancellationToken cancellationToken = default);
        Task<bool> DeleteBookingAdminAsync(int id, CancellationToken cancellationToken = default);
    }
}
