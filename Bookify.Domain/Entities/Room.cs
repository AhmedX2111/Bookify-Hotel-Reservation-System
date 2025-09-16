using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Domain.Entities
{
	public class Room
	{
		public int Id { get; set; }
		public string RoomNumber { get; set; } = null!;
		public int RoomTypeId { get; set; }
		public bool IsAvailable { get; set; } = true;

		// Navigation properties
		public virtual RoomType RoomType { get; set; } = null!;
		public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
	}
}
