using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
		public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public string? StripePaymentIntentId { get; set; }

		// Navigation properties
		public virtual User User { get; set; } = null!;
		public virtual Room Room { get; set; } = null!;
	}
}
