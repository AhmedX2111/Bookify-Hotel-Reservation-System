using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Domain.Entities
{
	public class RoomType
	{
		public int Id { get; set; }
		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;
		public decimal PricePerNight { get; set; }
		public int Capacity { get; set; }
		public string? ImageUrl { get; set; }

		// Navigation properties
		public ICollection<Room> Rooms { get; set; } = new List<Room>();
	}
}
