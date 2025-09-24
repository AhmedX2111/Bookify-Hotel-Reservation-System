using Bookify.Application.Business.Dtos.Rooms;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Application.Business.Rooms.Queries;
using Bookify.Application.Business.Services;
using Bookify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Bookify.Application.Business.Rooms.Queries.GetAvailableRoomsQuery;

namespace Bookify.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RoomsController : ControllerBase
	{

		private readonly IRoomService _roomService;

		public RoomsController(IRoomService roomService)
		{
			_roomService = roomService;
		}

		[HttpGet("available")]
		public async Task<ActionResult<IEnumerable<RoomDto>>> GetAvailableRooms(
			[FromQuery] DateTime checkInDate,
			[FromQuery] DateTime checkOutDate,
			CancellationToken cancellationToken = default)
		{
			try
			{
				if (checkInDate == default || checkOutDate == default)
				{
					return BadRequest("Both checkInDate and checkOutDate are required.");
				}

				if (checkInDate >= checkOutDate)
				{
					return BadRequest("Check-in date must be before check-out date.");
				}

				if (checkInDate < DateTime.Today)
				{
					return BadRequest("Check-in date cannot be in the past.");
				}

				var rooms = await _roomService.GetAvailableRoomsAsync(checkInDate, checkOutDate, cancellationToken);
				return Ok(rooms);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while retrieving available rooms.");
			}
		}

		[HttpGet("search")]
		public async Task<ActionResult<RoomSearchResultDto>> SearchAvailableRooms(
			[FromQuery] RoomSearchRequestDto request,
			CancellationToken cancellationToken = default)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var errors = request.GetValidationErrors();
				if (errors.Any())
				{
					return BadRequest(new { Message = "Invalid search parameters.", Errors = errors });
				}


				var result = await _roomService.SearchAvailableRoomsAsync(request, cancellationToken);
				return Ok(result);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while searching for rooms.");
			}
		}

		[HttpGet("roomtypes")]
		public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetRoomTypes(CancellationToken cancellationToken = default)
		{
			try
			{
				var roomTypes = await _roomService.GetRoomTypesAsync(cancellationToken);
				return Ok(roomTypes);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while retrieving room types.");
			}
		}
	}
}
