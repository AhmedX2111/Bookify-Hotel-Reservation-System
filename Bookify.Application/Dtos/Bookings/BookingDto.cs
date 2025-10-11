using System;

namespace Bookify.Application.Business.Dtos.Bookings
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        // 2. User & Room identifiers
        public string UserId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        // ===========================================

        // 3. Dates
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        // 4. Financial Details
        public int NumberOfNights { get; set; }
        public decimal TotalCost { get; set; }

        // 5. Status & Metadata
        public string Status { get; set; } // e.g., "Pending", "Confirmed", "Cancelled"


        public DateTime CreatedAt { get; set; }

        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public decimal? RefundAmount { get; set; }
        public decimal? CancellationFee { get; set; }
        public string RoomName { get; set; }
        public string RoomTypeName { get; set; }
    }
}