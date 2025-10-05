using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Bookify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomRepository _roomRepository;
        private const string CartSessionKey = "ReservationCart";

        public BookingsController(IBookingService bookingService, IRoomRepository roomRepository)
        {
            _bookingService = bookingService;
            _roomRepository = roomRepository;
        }

        // ----------------------------------------------------
        // 1. Reservation Cart & Booking Creation Flow
        // ----------------------------------------------------

        [HttpPost("cart")]
        public async Task<IActionResult> AddToCart([FromBody] BookingCreateDto bookingDetails)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate dates
            var numberOfNights = (int)(bookingDetails.CheckOutDate - bookingDetails.CheckInDate).TotalDays;
            if (numberOfNights <= 0)
                return BadRequest("Check-out date must be later than check-in date.");

            // Check if dates are in the future
            if (bookingDetails.CheckInDate.Date <= DateTime.Today)
                return BadRequest("Check-in date must be in the future.");

            // Validate room availability and existence
            var room = await _roomRepository.GetByIdAsync(bookingDetails.RoomId);
            if (room == null)
                return NotFound($"Room with ID {bookingDetails.RoomId} not found.");

            // Check room availability for the selected dates
            var isAvailable = await _bookingService.IsRoomAvailableAsync(
                bookingDetails.RoomId,
                bookingDetails.CheckInDate,
                bookingDetails.CheckOutDate);

            if (!isAvailable)
                return BadRequest("Room is not available for the selected dates.");

            var pricePerNight = room.RoomType?.PricePerNight ?? 100m;

            var cartItem = new CartItemDto
            {
                RoomId = room.Id,
                RoomNumber = room.RoomNumber,
                RoomTypeName = room.RoomType?.Name ?? "N/A",
                PricePerNight = pricePerNight,
                CheckInDate = bookingDetails.CheckInDate,
                CheckOutDate = bookingDetails.CheckOutDate,
                NumberOfNights = numberOfNights,
                TotalCost = numberOfNights * pricePerNight
            };

            var cartJson = JsonSerializer.Serialize(cartItem);
            HttpContext.Session.SetString(CartSessionKey, cartJson);

            return Ok(new
            {
                Message = "Room added to reservation cart successfully.",
                CartItem = cartItem
            });
        }

        [HttpGet("cart")]
        public IActionResult GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return Ok(new { Message = "Cart is empty" });
            }

            var cartItem = JsonSerializer.Deserialize<CartItemDto>(cartJson);
            return Ok(cartItem);
        }

        [HttpDelete("cart")]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return Ok(new { Message = "Reservation cart cleared." });
        }

        // ----------------------------------------------------
        // 2. Complete Booking Confirmation Sequence
        // ----------------------------------------------------

        [HttpPost("confirm")]
        public async Task<ActionResult<BookingDto>> ConfirmBookingFromCart(CancellationToken cancellationToken)
        {
            // Step 1: Validate cart exists
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return BadRequest(new { Message = "Cannot confirm booking. Cart is empty." });
            }

            // Step 2: Validate user authentication
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated correctly.");
            }

            var cartItem = JsonSerializer.Deserialize<CartItemDto>(cartJson);

            try
            {
                // Step 3: Revalidate availability before confirmation
                var isAvailable = await _bookingService.IsRoomAvailableAsync(
                    cartItem.RoomId,
                    cartItem.CheckInDate,
                    cartItem.CheckOutDate);

                if (!isAvailable)
                {
                    // Clear cart since room is no longer available
                    HttpContext.Session.Remove(CartSessionKey);
                    return BadRequest("Room is no longer available for the selected dates. Please choose different dates.");
                }

                // Step 4: Convert cart to booking
                var bookingCreateDto = new BookingCreateDto
                {
                    RoomId = cartItem.RoomId,
                    CheckInDate = cartItem.CheckInDate,
                    CheckOutDate = cartItem.CheckOutDate
                };

                // Step 5: Create booking (status will be Pending initially)
                var bookingDto = await _bookingService.CreateBookingAsync(userId, bookingCreateDto, cancellationToken);

                // Step 6: Clear cart after successful booking
                HttpContext.Session.Remove(CartSessionKey);

                // Step 7: Return booking confirmation
                return CreatedAtAction(nameof(GetBookingById), new { id = bookingDto.Id }, new
                {
                    Message = "Booking created successfully! It is now pending confirmation.",
                    Booking = bookingDto,
                    NextSteps = new[]
                    {
                        "Your booking is pending admin confirmation",
                        "You will receive a confirmation email once approved",
                        "You can cancel the booking before it's confirmed"
                    }
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new
                {
                    Message = "An error occurred while confirming the booking.",
                    Details = ex.Message
                });
            }
        }

        // ----------------------------------------------------
        // 3. Enhanced Booking Management with Status Flow
        // ----------------------------------------------------

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetUserBookings(CancellationToken cancellationToken)
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (string.IsNullOrEmpty(userId))
            //{
            //    return Unauthorized();
            //}

            var bookings = await _bookingService.GetUserBookingsAsync("8bbf1138-2d6b-4300-8df4-d155a3654739", cancellationToken);
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetBookingById(int id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id, userId, cancellationToken);
                return Ok(booking);
            }
            catch (NotFoundException)
            {
                return NotFound(new { Message = $"Booking with ID {id} not found." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You don't have access to this booking.");
            }
        }

        // ----------------------------------------------------
        // 4. Comprehensive Cancellation Sequence
        // ----------------------------------------------------

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingRequest cancelRequest, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Step 1: Validate cancellation reason
                if (string.IsNullOrWhiteSpace(cancelRequest?.Reason))
                {
                    return BadRequest("Cancellation reason is required.");
                }

                // Step 2: Attempt cancellation
                var result = await _bookingService.CancelBookingAsync(id, userId, cancelRequest.Reason, cancellationToken);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Message = $"Booking {id} has been cancelled successfully.",
                        RefundAmount = result.RefundAmount,
                        CancellationFee = result.CancellationFee,
                        Status = "Cancelled"
                    });
                }

                return BadRequest(new { Message = result.ErrorMessage });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Booking cannot be cancelled (too late, already checked in, etc.)
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ----------------------------------------------------
        // 5. Admin Confirmation Sequence
        // ----------------------------------------------------

        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmBooking(int id, CancellationToken cancellationToken)
        {
            try
            {
                // Step 1: Confirm booking
                var result = await _bookingService.ConfirmBookingAsync(id, cancellationToken);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Message = $"Booking {id} has been confirmed.",
                        Booking = result.BookingDetails,
                        NextSteps = new[]
                        {
                            "Confirmation email sent to customer",
                            "Room allocated for the booking dates",
                            "Booking is now active"
                        }
                    });
                }

                return BadRequest(new { Message = result.ErrorMessage });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ----------------------------------------------------
        // 6. Additional Admin Endpoints for Complete Flow
        // ----------------------------------------------------

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectBooking(int id, [FromBody] RejectBookingRequest rejectRequest, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rejectRequest?.Reason))
                {
                    return BadRequest("Rejection reason is required.");
                }

                var result = await _bookingService.RejectBookingAsync(id, rejectRequest.Reason, cancellationToken);

                if (result)
                {
                    return Ok(new
                    {
                        Message = $"Booking {id} has been rejected.",
                        Reason = rejectRequest.Reason
                    });
                }

                return StatusCode(500, "Could not reject booking.");
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetBookingStatus(int id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var statusInfo = await _bookingService.GetBookingStatusAsync(id, userId, cancellationToken);
                return Ok(statusInfo);
            }
            catch (NotFoundException)
            {
                return NotFound(new { Message = $"Booking with ID {id} not found." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You don't have access to this booking.");
            }
        }
    }

    // Supporting DTOs
    public class CancelBookingRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class RejectBookingRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}