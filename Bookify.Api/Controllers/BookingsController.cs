// Bookify-Hotel-Reservation-System-master/Bookify.Api/Controllers/BookingsController.cs
using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization; // Added
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System; // Added
using System.Collections.Generic; // Added
using System.Security.Claims; // Added
using System.Threading; // Added
using System.Threading.Tasks;

namespace Bookify.Api.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]
    //[Authorize] // All booking operations require authentication
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService; // Injected IBookingService

        public BookingsController(IBookingService bookingService) // Updated constructor
        {
            _bookingService = bookingService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // --- Reservation Cart Endpoints ---
        [HttpPost("cart")]
        public async Task<ActionResult> AddToCart([FromBody] BookingCreateDto request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated.");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Basic date validation
            if (request.CheckInDate.Date < DateTime.Today)
            {
                return BadRequest("Check-in date cannot be in the past.");
            }
            if (request.CheckInDate >= request.CheckOutDate)
            {
                return BadRequest("Check-out date must be later than check-in date.");
            }

            var success = await _bookingService.AddToReservationCartAsync(userId, request, cancellationToken);
            if (!success) return BadRequest("Failed to add item to cart, or item already exists, or room is unavailable.");

            return Ok("Item added to cart successfully.");
        }

        [HttpGet("cart")]
        public async Task<ActionResult<List<BookingCreateDto>>> GetCart(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated.");

            var cart = await _bookingService.GetReservationCartAsync(userId, cancellationToken);
            return Ok(cart);
        }

        [HttpDelete("cart/{roomId}")]
        public async Task<ActionResult> RemoveFromCart(int roomId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated.");

            var success = await _bookingService.RemoveFromReservationCartAsync(userId, roomId, cancellationToken);
            if (!success) return NotFound("Item not found in cart.");

            return NoContent();
        }
        // --- End Reservation Cart Endpoints ---
    }
}
