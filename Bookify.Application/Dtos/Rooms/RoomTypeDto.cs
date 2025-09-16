using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Rooms
{
	public class RoomTypeDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = null!;
		public string Description { get; set; } = null!;
		public decimal PricePerNight { get; set; }
		public int Capacity { get; set; }
	}
}
