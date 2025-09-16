using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Rooms
{
	public class RoomSearchResultDto
	{
		public List<RoomDto> Rooms { get; set; } = new List<RoomDto>();
		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int TotalPages { get; set; }
		public bool HasPrevious => PageNumber > 1;
		public bool HasNext => PageNumber < TotalPages;
	}
}
