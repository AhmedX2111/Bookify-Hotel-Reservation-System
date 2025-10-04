// Bookify.Application/Dtos/Bookings/BookingDto.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Bookings
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!; // Added for display
        public string UserEmail { get; set; } = null!; // Added for display
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = null!; // Added for display
        public string RoomTypeName { get; set; } = null!; // Added for display
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfNights { get; set; }
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? StripePaymentIntentId { get; set; }
    }
}
