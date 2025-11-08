using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Domain.Entities;

namespace Bookify.Application.Business.Interfaces.Services
{
    /// <summary>
    /// Service interface for managing bookings, including creation, retrieval, and status updates.
    /// </summary>
    public interface IBookingService
    {
        /// <summary>
        /// Creates a new booking for a user.
        /// </summary>
        /// <param name="userId">The ID of the user creating the booking.</param>
        /// <param name="bookingCreateDto">The booking creation details.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>The created booking DTO on success.</returns>
        Task<BookingDto> CreateBookingAsync(string userId, BookingCreateDto bookingCreateDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all bookings for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>A collection of user bookings.</returns>
      //  Task<IEnumerable<BookingDto>> GetUser BookingsAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific booking by ID, with user access check.
        /// </summary>
        /// <param name="id">The booking ID.</param>
        /// <param name="userId">The ID of the user (for authorization).</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>The booking DTO if found and authorized; otherwise null.</returns>
        Task<BookingDto?> GetBookingByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Simple cancellation of a booking (returns success/failure only).
        /// </summary>
        /// <param name="bookingId">The booking ID.</param>
        /// <param name="userId">The ID of the user canceling (for authorization).</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>True if canceled successfully; false otherwise.</returns>
        Task<CancellationResult> CancelBookingAsync(int id, string userId, string reason, CancellationToken cancellationToken);

        /// <summary>
        /// Detailed cancellation of a booking with reason and refund info.
        /// </summary>
        /// <param name="id">The booking ID.</param>
        /// <param name="userId">The ID of the user canceling (for authorization).</param>
        /// <param name="reason">The cancellation reason.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>A result object with success status, error message, and financial details.</returns>
        Task<CancellationResult> CancelBookingWithReasonAsync(int id, string userId, string reason, CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirms a booking (manager/admin only).
        /// </summary>
        /// <param name="bookingId">The booking ID.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>A result object with success status, error message, and booking details.</returns>
        Task<ConfirmationResult> ConfirmBookingAsync(int bookingId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rejects a booking with a reason (manager/admin only).
        /// </summary>
        /// <param name="id">The booking ID.</param>
        /// <param name="reason">The rejection reason.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>A result object with success status and error message.</returns>
        Task<bool> RejectBookingAsync(int id, string reason, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all bookings (admin/manager view).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>A collection of all bookings.</returns>
        Task<IEnumerable<BookingDto>> GetAllBookingsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a room is available for the given dates.
        /// </summary>
        /// <param name="roomId">The room ID.</param>
        /// <param name="checkIn">The check-in date.</param>
        /// <param name="checkOut">The check-out date.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>True if the room is available; false otherwise.</returns>
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets detailed status information for a booking.
        /// </summary>
        /// <param name="id">The booking ID.</param>
        /// <param name="userId">The ID of the user (for authorization).</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>Booking status info if found and authorized; otherwise throws or returns empty.</returns>
        Task<BookingStatusInfo> GetBookingStatusAsync(int id, string userId, CancellationToken cancellationToken = default);

        Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId, CancellationToken cancellationToken);

        Task<string> ProcessPaymentAsync(decimal amount, string paymentMethodId);

        /// <summary>
        /// Creates a booking and processes payment in a single atomic operation using Unit of Work pattern.
        /// </summary>
        Task<BookingDto> CreateBookingWithPaymentAsync(string userId, BookingCreateDto bookingDto, string paymentMethodId, CancellationToken cancellationToken = default);

    }

    /// <summary>
    /// Result for cancellation operations, including financial details.
    /// </summary>
    public class CancellationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal CancellationFee { get; set; }
    }

    /// <summary>
    /// Result for confirmation/rejection operations.
    /// </summary>
    public class ConfirmationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public object? BookingDetails { get; set; } // Can be BookingDto or similar
    }

    /// <summary>
    /// Detailed status information for a booking.
    /// </summary>
    public class BookingStatusInfo
    {
        public string Status { get; set; } = string.Empty; // e.g., "Confirmed"
        public bool CanCancel { get; set; }
        public string CancellationPolicy { get; set; } = string.Empty;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
