using System.ComponentModel.DataAnnotations;


namespace Bookify.Domain.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfNights { get; set; }
        public decimal TotalCost { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed
        public string? PaymentIntentId { get; set; }  // Stripe PaymentIntent ID (nullable until payment is processed)

        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public decimal? RefundAmount { get; set; }
        public decimal? CancellationFee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
    }


    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Active,
        Completed,
        Cancelled,
        Rejected
    }
}
