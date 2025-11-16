using System;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.Business.Dtos.Bookings
{
    public class BookingCreateDto
    {
        [Required(ErrorMessage = "RoomId is required.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "CheckInDate is required.")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "CheckOutDate is required.")]
        public DateTime CheckOutDate { get; set; }

        // Customer info is optional - will be retrieved from authenticated user if not provided
        [StringLength(100, ErrorMessage = "Guest name cannot exceed 100 characters.")]
        public string? CustomerName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? CustomerEmail { get; set; }

        // For now, we rely on the service to calculate TotalCost and NumberOfNights
    }
}