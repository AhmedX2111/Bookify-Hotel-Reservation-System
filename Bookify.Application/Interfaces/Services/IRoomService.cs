using Bookify.Application.Business.Dtos.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Services
{
	public interface IRoomService
	{
		/// <summary>
		/// Retrieves available rooms for a given date range
		/// </summary>
		Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(
			DateTime checkInDate,
			DateTime checkOutDate,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Searches available rooms with filters and pagination
		/// </summary>
		Task<RoomSearchResultDto> SearchAvailableRoomsAsync(
			RoomSearchRequestDto request,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets all room types
		/// </summary>
		Task<IEnumerable<RoomTypeDto>> GetRoomTypesAsync(CancellationToken cancellationToken = default);

	}
}
