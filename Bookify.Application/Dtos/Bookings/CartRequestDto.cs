using System;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.Business.Dtos.Bookings
{
    public class CartRequestDto
    {
        [Required(ErrorMessage = "RoomId is required.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "CheckInDate is required.")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "CheckOutDate is required.")]
        public DateTime CheckOutDate { get; set; }
    }
}


