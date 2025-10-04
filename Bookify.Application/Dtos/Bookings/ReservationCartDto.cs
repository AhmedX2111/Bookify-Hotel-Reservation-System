using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Bookings
{
	public class ReservationCartDto
	{
		public List<ReservationCartItemDto> Items { get; set; } = new();
		public decimal TotalAmount { get; set; }
	}
}
