using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Rooms
{
	public class RoomSearchRequestDto
	{
		public DateTime CheckInDate { get; set; }
		public DateTime CheckOutDate { get; set; }
		public int? RoomTypeId { get; set; }
		public int? MinCapacity { get; set; }
		public decimal? MaxPrice { get; set; }
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string? SortBy { get; set; } = "PricePerNight";
		public bool SortDescending { get; set; } = false;

		public bool IsValid()
		{
			return CheckInDate < CheckOutDate &&
				   CheckInDate >= DateTime.Today &&
				   PageNumber > 0 &&
				   PageSize > 0 &&
				   PageSize <= 100;
		}
	}
}
