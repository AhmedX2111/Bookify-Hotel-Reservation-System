using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Bookings
{
	public class ReservationCartItemDto
	{
		public int RoomId { get; set; }
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public int NumberOfGuests { get; set; }
		public int NumberOfNights { get; set; }
		public decimal TotalPrice { get; set; }
		public string RoomName { get; set; } = string.Empty;
		public string RoomType { get; set; } = string.Empty;
	}
}
