// Bookify-Hotel-Reservation-System-master/Bookify.Api/Controllers/AdminController.cs
using Bookify.Application.Business.Dtos.Bookings; // Added
using Bookify.Application.Business.Dtos.Rooms; // Added
using Bookify.Application.Business.Interfaces.Services; // Added
using Microsoft.AspNetCore.Authorization; // Added
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System; // Added
using System.Collections.Generic; // Added
using System.Threading; // Added
using System.Threading.Tasks;

namespace Bookify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")] // Restrict access to Admin role
    public class AdminController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;

        public AdminController(IRoomService roomService, IBookingService bookingService)
        {
            _roomService = roomService;
            _bookingService = bookingService;
        }

        // --- Room Management ---
        [HttpGet("rooms")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAllRooms(CancellationToken cancellationToken)
        {
            var rooms = await _roomService.GetAllRoomsAdminAsync(cancellationToken);
            return Ok(rooms);
        }

        [HttpGet("rooms/{id}")]
        public async Task<ActionResult<RoomDto>> GetRoomById(int id, CancellationToken cancellationToken)
        {
            var room = await _roomService.GetRoomByIdAdminAsync(id, cancellationToken);
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpPost("rooms")]
        public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] RoomCreateUpdateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var newRoom = await _roomService.CreateRoomAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetRoomById), new { id = newRoom.Id }, newRoom);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("rooms/{id}")]
        public async Task<ActionResult> UpdateRoom(int id, [FromBody] RoomCreateUpdateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var success = await _roomService.UpdateRoomAsync(id, request, cancellationToken);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("rooms/{id}")]
        public async Task<ActionResult> DeleteRoom(int id, CancellationToken cancellationToken)
        {
            var success = await _roomService.DeleteRoomAsync(id, cancellationToken);
            if (!success) return NotFound();
            return NoContent();
        }

        // --- Room Type Management ---
        [HttpGet("roomtypes")]
        public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetAllRoomTypes(CancellationToken cancellationToken)
        {
            var roomTypes = await _roomService.GetRoomTypesAsync(cancellationToken); // Reusing existing method
            return Ok(roomTypes);
        }

        [HttpGet("roomtypes/{id}")]
        public async Task<ActionResult<RoomTypeDto>> GetRoomTypeById(int id, CancellationToken cancellationToken)
        {
            var roomType = await _roomService.GetRoomTypeByIdAdminAsync(id, cancellationToken);
            if (roomType == null) return NotFound();
            return Ok(roomType);
        }

        [HttpPost("roomtypes")]
        public async Task<ActionResult<RoomTypeDto>> CreateRoomType([FromBody] RoomTypeCreateUpdateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var newRoomType = await _roomService.CreateRoomTypeAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetRoomTypeById), new { id = newRoomType.Id }, newRoomType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("roomtypes/{id}")]
        public async Task<ActionResult> UpdateRoomType(int id, [FromBody] RoomTypeCreateUpdateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var success = await _roomService.UpdateRoomTypeAsync(id, request, cancellationToken);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("roomtypes/{id}")]
        public async Task<ActionResult> DeleteRoomType(int id, CancellationToken cancellationToken)
        {
            var success = await _roomService.DeleteRoomTypeAsync(id, cancellationToken);
            if (!success) return NotFound();
            return NoContent();
        }

        // --- Booking Management ---
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAllBookings(
            [FromQuery] string? status,
            [FromQuery] DateTime? checkInFrom,
            [FromQuery] DateTime? checkInTo,
            CancellationToken cancellationToken)
        {
            var bookings = await _bookingService.GetAllBookingsAdminAsync(status, checkInFrom, checkInTo, cancellationToken);
            return Ok(bookings);
        }

        [HttpGet("bookings/{id}")]
        public async Task<ActionResult<BookingDto>> GetBookingById(int id, CancellationToken cancellationToken)
        {
            var booking = await _bookingService.GetBookingByIdAdminAsync(id, cancellationToken);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpPut("bookings/{id}/status")]
        public async Task<ActionResult> UpdateBookingStatus(int id, [FromBody] string newStatus, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(newStatus)) return BadRequest("New status is required.");
            try
            {
                var success = await _bookingService.UpdateBookingStatusAdminAsync(id, newStatus, cancellationToken);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("bookings/{id}")]
        public async Task<ActionResult> DeleteBooking(int id, CancellationToken cancellationToken)
        {
            var success = await _bookingService.DeleteBookingAdminAsync(id, cancellationToken);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
