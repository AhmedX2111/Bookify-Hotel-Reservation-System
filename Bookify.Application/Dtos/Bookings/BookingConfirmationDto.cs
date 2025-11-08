using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Bookings
{
    public class BookingConfirmationDto
    {
        [Required]
        public int RoomId { get; set; }
        [Required]
        public DateTime CheckInDate { get; set; }
        [Required]
        public DateTime CheckOutDate { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        [Required]
        public string PaymentMethodId { get; set; }  // From Stripe client
    }
}
