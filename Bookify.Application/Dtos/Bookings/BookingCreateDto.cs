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

        [Required(ErrorMessage = "Guest name is required.")]
        [StringLength(100, ErrorMessage = "Guest name cannot exceed 100 characters.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Guest email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string CustomerEmail { get; set; } = string.Empty;

        // For now, we rely on the service to calculate TotalCost and NumberOfNights
    }
}