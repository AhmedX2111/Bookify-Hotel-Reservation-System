using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Data.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookify.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize(Roles = "Customer")]
	public class BookingsController : ControllerBase
	{
		//private readonly IUnitOfWork _unitOfWork;
		//private readonly IReservationCartService _cartService;
		//private readonly IHttpContextAccessor _httpContextAccessor;

		//public BookingsController(IUnitOfWork unitOfWork, IReservationCartService cartService, IHttpContextAccessor httpContextAccessor)
		//{
		//	_unitOfWork = unitOfWork;
		//	_cartService = cartService;
		//	_httpContextAccessor = httpContextAccessor;
		//}

		//private string GetSessionId()
		//{
		//	var session = _httpContextAccessor.HttpContext?.Session;
		//	if (session == null)
		//		throw new InvalidOperationException("Session is not available. Make sure session middleware is enabled.");

		//	return session.Id;
		//}

		//// ✅ Add room to session cart
		//[HttpPost("add-to-cart")]
		//public async Task<IActionResult> AddToCart([FromBody] ReservationCartItemDto item)
		//{
		//	if (item == null) return BadRequest("Invalid cart item");

		//	var sessionId = GetSessionId();
		//	await _cartService.AddToCartAsync(sessionId, item);
		//	return Ok(new { message = "Room added to cart" });
		//}

		//// ✅ Get cart from session
		//[HttpGet("cart")]
		//public async Task<IActionResult> GetCart()
		//{
		//	var sessionId = GetSessionId();
		//	var cart = await _cartService.GetCartAsync(sessionId);
		//	return Ok(cart);
		//}

		//// ✅ Clear cart
		//[HttpDelete("clear-cart")]
		//public async Task<IActionResult> ClearCart()
		//{
		//	var sessionId = GetSessionId();
		//	await _cartService.ClearCartAsync(sessionId);
		//	return Ok(new { message = "Cart cleared" });
		//}

		//// ✅ Checkout requires login
		//[Authorize(Roles = "Customer")]
		//[HttpPost("checkout")]
		//public async Task<IActionResult> Checkout()
		//{
		//	var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		//	var sessionId = GetSessionId();
		//	var cart = await _cartService.GetCartAsync(sessionId);

		//	if (!cart.Items.Any())
		//		return BadRequest("Cart is empty");

		//	foreach (var item in cart.Items)
		//	{
		//		var booking = new Booking
		//		{
		//			UserId = userId,
		//			RoomId = item.RoomId, // ensure RoomId type matches domain
		//			CheckInDate = item.CheckInDate,
		//			CheckOutDate = item.CheckOutDate,
		//			NumberOfNights = (item.CheckOutDate - item.CheckInDate).Days,
		//			TotalCost = item.TotalPrice,
		//			Status = "Pending",
		//			CreatedAt = DateTime.UtcNow
		//		};

		//		await _unitOfWork.Bookings.AddAsync(booking);
		//	}

		//	await _unitOfWork.SaveChangesAsync();

		//	// Empty cart after checkout
		//	await _cartService.ClearCartAsync(sessionId);

		//	return Ok(new { message = "Booking(s) created successfully" });
		//}

		//// ✅ List user's own bookings
		//[Authorize(Roles = "Customer")]
		//[HttpGet("my-bookings")]
		//public async Task<IActionResult> MyBookings()
		//{
		//	var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		//	var bookings = await _unitOfWork.Bookings.GetUserBookingsAsync(userId);
		//	return Ok(bookings);
		//}

		private readonly IReservationCartService _cartService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<User> _userManager;
		private const string SessionKey = "ReservationCartSession";

		public BookingsController(IReservationCartService cartService, IUnitOfWork unitOfWork, UserManager<User> userManager)
		{
			_cartService = cartService;
			_unitOfWork = unitOfWork;
			_userManager = userManager;
		}

		//// Helper to get or create sessionId
		//private string GetSessionId()
		//{
		//	if (!HttpContext.Session.TryGetValue(SessionKey, out _))
		//	{
		//		var sessionId = Guid.NewGuid().ToString();
		//		HttpContext.Session.SetString(SessionKey, sessionId);
		//	}
		//	return HttpContext.Session.GetString(SessionKey)!;
		//}

		//// GET: api/bookings/cart
		//[HttpGet("cart")]
		//public async Task<IActionResult> GetCart()
		//{
		//	var sessionId = GetSessionId();
		//	var cart = await _cartService.GetCartAsync(sessionId);
		//	return Ok(cart);
		//}

		//// POST: api/bookings/cart/add
		//[HttpPost("cart/add")]
		//public async Task<IActionResult> AddToCart([FromBody] ReservationCartItemDto item)
		//{
		//	if (item == null)
		//		return BadRequest("Item is required.");

		//	var sessionId = GetSessionId();
		//	await _cartService.AddToCartAsync(sessionId, item);
		//	return Ok(new { Message = "Item added to cart successfully" });
		//}

		//// DELETE: api/bookings/cart/remove/{roomId}
		//[HttpDelete("cart/remove/{roomId}")]
		//public async Task<IActionResult> RemoveFromCart(int roomId)
		//{
		//	var sessionId = GetSessionId();
		//	await _cartService.RemoveFromCartAsync(sessionId, roomId);
		//	return Ok(new { Message = "Item removed from cart successfully" });
		//}

		//// DELETE: api/bookings/cart/clear
		//[HttpDelete("cart/clear")]
		//public async Task<IActionResult> ClearCart()
		//{
		//	var sessionId = GetSessionId();
		//	await _cartService.ClearCartAsync(sessionId);
		//	return Ok(new { Message = "Cart cleared successfully" });
		//}

		// POST: api/bookings/cart/confirm
		//[HttpPost("cart/confirm")]
		//public async Task<IActionResult> ConfirmBooking()
		//{
		//	//var sessionId = GetSessionId();

		//	//// Get the authenticated user's entity
		//	//var user = await _userManager.GetUserAsync(User);
		//	//if (user == null)
		//	//	return Unauthorized("User not found");

		//	//if (string.IsNullOrEmpty(user.Id))
		//	//	return Unauthorized(new { Message = "You must be logged in to confirm booking." });

		//	//var success = await _cartService.ConfirmBookingAsync(sessionId, user.Id);

		//	//if (!success)
		//	//	return BadRequest(new { Message = "Booking confirmation failed" });

		//	//// Clear the session cart after confirmation
		//	//await _cartService.ClearCartAsync(sessionId);

		//	//return Ok(new { Message = "Booking confirmed successfully" });

		//	var sessionId = GetSessionId();

		//	// Direct claim access - most reliable for JWT
		//	var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
		//				?? User.FindFirstValue("sub");

		//	if (string.IsNullOrEmpty(userId))
		//		return Unauthorized("You must be logged in to confirm booking.");

		//	var success = await _cartService.ConfirmBookingAsync(sessionId, userId);

		//	if (!success)
		//		return BadRequest(new { Message = "Booking confirmation failed" });

		//	await _cartService.ClearCartAsync(sessionId);
		//	return Ok(new { Message = "Booking confirmed successfully" });
		//}

		//private readonly IUnitOfWork _unitOfWork;
		//private readonly UserManager<User> _userManager;

		//public BookingsController(IUnitOfWork unitOfWork, UserManager<User> userManager)
		//{
		//	_unitOfWork = unitOfWork;
		//	_userManager = userManager;
		//}

		// ✅ List user's own bookings
		[Authorize(Roles = "Customer")]
		[HttpGet("my-bookings")]
		public async Task<IActionResult> MyBookings()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var bookings = await _unitOfWork.Bookings.GetUserBookingsAsync(userId);
			return Ok(bookings);
		}

		// ✅ Get booking by ID
		[Authorize]
		[HttpGet("{id}")]
		public async Task<IActionResult> GetBooking(int id)
		{
			var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
			if (booking == null)
				return NotFound();

			// Check if user owns this booking or is admin
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var isAdmin = User.IsInRole("Admin");

			if (!isAdmin && booking.UserId != userId)
				return Forbid();

			return Ok(booking);
		}

		// ✅ Cancel booking
		[Authorize(Roles = "Customer")]
		[HttpPut("{id}/cancel")]
		public async Task<IActionResult> CancelBooking(int id)
		{
			var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
			if (booking == null)
				return NotFound();

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (booking.UserId != userId)
				return Forbid();

			booking.Status = "Cancelled";
			await _unitOfWork.SaveChangesAsync();

			return Ok(new { message = "Booking cancelled successfully" });
		}

		// ✅ Update booking status (Admin only)
		[Authorize(Roles = "Admin")]
		[HttpPut("{id}/status")]
		public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] string status)
		{
			var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
			if (booking == null)
				return NotFound();

			booking.Status = status;
			await _unitOfWork.SaveChangesAsync();

			return Ok(new { message = "Booking status updated successfully" });
		}

		// ✅ Get all bookings (Admin only)
		[Authorize(Roles = "Admin")]
		[HttpGet]
		public async Task<IActionResult> GetAllBookings()
		{
			var bookings = await _unitOfWork.Bookings.GetAllAsync();
			return Ok(bookings);
		}
	}
}
