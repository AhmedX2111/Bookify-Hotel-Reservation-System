using Bookify.Application.Business.Dtos.Rooms;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Rooms.Queries
{
	public record GetAvailableRoomsQuery(DateTime CheckInDate, DateTime CheckOutDate) : IRequest<IEnumerable<RoomDto>>;
	
}
