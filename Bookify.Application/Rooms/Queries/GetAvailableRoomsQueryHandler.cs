using AutoMapper;
using Bookify.Application.Business.Dtos.Rooms;
using Bookify.Application.Business.Interfaces.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Rooms.Queries
{
	public class GetAvailableRoomsQueryHandler : IRequestHandler<GetAvailableRoomsQuery, IEnumerable<RoomDto>>
	{
		private readonly IBookfiyDbContext _context;
		private readonly IMapper _mapper;

		public GetAvailableRoomsQueryHandler(IBookfiyDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<IEnumerable<RoomDto>> Handle(GetAvailableRoomsQuery request, CancellationToken cancellationToken)
		{
			// Get rooms that are not booked for the given date range
			var bookedRoomIds = await _context.Bookings
				.Where(b =>
					(b.CheckInDate <= request.CheckOutDate && b.CheckOutDate >= request.CheckInDate) &&
					(b.Status == "Confirmed" || b.Status == "Pending"))
				.Select(b => b.RoomId)
				.Distinct()
				.ToListAsync(cancellationToken);

			var availableRooms = await _context.Rooms
				.Include(r => r.RoomType)
				.Where(r => !bookedRoomIds.Contains(r.Id) && r.IsAvailable)
				.ToListAsync(cancellationToken);

			return _mapper.Map<IEnumerable<RoomDto>>(availableRooms);
		}
	}
}
