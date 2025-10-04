using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bookify.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize]
	public class ReservationCartController : ControllerBase
	{
		//private readonly IReservationCartService _cartService;
		//private readonly IHttpContextAccessor _httpContextAccessor;
		//private readonly UserManager<User> _userManager;

		//private const string CartSessionKey = "ReservationCart";

		//public ReservationCartController(IReservationCartService cartService, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
		//{
		//	_cartService = cartService;
		//	_httpContextAccessor = httpContextAccessor;
		//	_userManager = userManager;
		//}

		//private string GetSessionId()
		//{
		//	var session = _httpContextAccessor.HttpContext?.Session;
		//	if (session == null)
		//		throw new InvalidOperationException("Session is not available. Make sure session middleware is enabled.");

		//	// Use session ID as unique key
		//	return session.Id;
		//}

		//[HttpGet]
		//public async Task<ActionResult<ReservationCartDto>> GetCart()
		//{
		//	var sessionId = GetSessionId();
		//	var cart = await _cartService.GetCartAsync(sessionId);
		//	return Ok(cart);
		//}

		//[HttpPost("add")]
		//public async Task<IActionResult> AddToCart([FromBody] ReservationCartItemDto item)
		//{
		//	var sessionId = GetSessionId();
		//	await _cartService.AddToCartAsync(sessionId, item);
		//	return Ok(new { message = "Item added to cart." });
		//}

		//[HttpDelete("remove/{roomId}")]
		//public async Task<IActionResult> RemoveFromCart(int roomId)
		//{
		//	var sessionId = GetSessionId();
		//	await _cartService.RemoveFromCartAsync(sessionId, roomId);
		//	return Ok(new { message = "Item removed from cart." });
		//}

		//[HttpPost("confirm")]
		//public async Task<IActionResult> ConfirmBooking()
		//{
		//	var sessionId = GetSessionId();

		//	// Debug: Check what claims are available
		//	var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
		//	Console.WriteLine("Available claims:");
		//	foreach (var claim in claims)
		//	{
		//		Console.WriteLine($"{claim.Type}: {claim.Value}");
		//	}

		//	// Get the authenticated user's ID using multiple methods
		//	var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
		//				?? User.FindFirstValue("sub");

		//	if (string.IsNullOrEmpty(userId))
		//	{
		//		// Fallback: try to get user by email
		//		var userEmail = User.FindFirstValue(ClaimTypes.Email);
		//		if (!string.IsNullOrEmpty(userEmail))
		//		{
		//			var user = await _userManager.FindByEmailAsync(userEmail);
		//			userId = user?.Id;
		//		}
		//	}

		//	if (string.IsNullOrEmpty(userId))
		//		return Unauthorized("You must be logged in to confirm booking.");

		//	Console.WriteLine($"Using UserId: {userId}");

		//	var result = await _cartService.ConfirmBookingAsync(sessionId, userId);

		//	if (!result)
		//		return BadRequest("Booking confirmation failed");

		//	// Clear the session cart after confirmation
		//	await _cartService.ClearCartAsync(sessionId);

		//	return Ok(new { message = "Booking confirmed successfully" });
		//}

		//[HttpDelete("clear")]
		//public async Task<IActionResult> ClearCart()
		//{
		//	var sessionId = GetSessionId();
		//	await _cartService.ClearCartAsync(sessionId);
		//	return Ok(new { message = "Cart cleared." });
		//}

		private readonly IReservationCartService _cartService;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<User> _userManager;

		private const string CartSessionKey = "ReservationCart";

		public ReservationCartController(IReservationCartService cartService,
									   IHttpContextAccessor httpContextAccessor,
									   UserManager<User> userManager)
		{
			_cartService = cartService;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
		}

		private string GetSessionId()
		{
			var session = _httpContextAccessor.HttpContext?.Session;
			if (session == null)
				throw new InvalidOperationException("Session is not available. Make sure session middleware is enabled.");

			return session.Id;
		}

		[HttpGet]
		public async Task<ActionResult<ReservationCartDto>> GetCart()
		{
			var sessionId = GetSessionId();
			var cart = await _cartService.GetCartAsync(sessionId);
			return Ok(cart);
		}

		[HttpPost("add")]
		public async Task<IActionResult> AddToCart([FromBody] ReservationCartItemDto item)
		{
			var sessionId = GetSessionId();
			await _cartService.AddToCartAsync(sessionId, item);
			return Ok(new { message = "Item added to cart." });
		}

		[HttpDelete("remove/{roomId}")]
		public async Task<IActionResult> RemoveFromCart(int roomId)
		{
			var sessionId = GetSessionId();
			await _cartService.RemoveFromCartAsync(sessionId, roomId);
			return Ok(new { message = "Item removed from cart." });
		}

		[Authorize]
		[HttpPost("confirm")]
		public async Task<IActionResult> ConfirmBooking()
		{
			try
			{
				var sessionId = GetSessionId();
				Console.WriteLine($"=== DEBUG CONFIRM BOOKING STARTED ===");
				Console.WriteLine($"SessionId: {sessionId}");

				// Debug: Check ALL claims
				var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
				Console.WriteLine("=== ALL CLAIMS IN USER ===");
				foreach (var claim in claims)
				{
					Console.WriteLine($"{claim.Type}: {claim.Value}");
				}

				// Get ALL nameidentifier claims (there are two!)
				var nameIdentifierClaims = User.Claims
					.Where(c => c.Type == ClaimTypes.NameIdentifier)
					.Select(c => c.Value)
					.ToList();

				Console.WriteLine($"🔍 All NameIdentifier claims:");
				foreach (var claimValue in nameIdentifierClaims)
				{
					Console.WriteLine($"  - '{claimValue}' (length: {claimValue.Length})");
				}

				// The REAL user ID is the GUID, not the username
				string finalUserId = null;

				foreach (var claimValue in nameIdentifierClaims)
				{
					// Look for the GUID format (contains hyphens and is longer)
					if (claimValue.Contains("-") && claimValue.Length > 20)
					{
						finalUserId = claimValue;
						Console.WriteLine($"✅ Found GUID UserId: {finalUserId}");
						break;
					}
				}

				// If no GUID found, try other methods
				if (finalUserId == null)
				{
					var email = User.FindFirstValue(ClaimTypes.Email);
					if (!string.IsNullOrEmpty(email))
					{
						var userByEmail = await _userManager.FindByEmailAsync(email);
						if (userByEmail != null)
						{
							finalUserId = userByEmail.Id;
							Console.WriteLine($"✅ Found UserId by email: {finalUserId}");
						}
					}
				}

				if (string.IsNullOrEmpty(finalUserId))
				{
					Console.WriteLine("❌ No valid user ID found");
					return Unauthorized("You must be logged in to confirm booking.");
				}

				Console.WriteLine($"✅ Final UserId: '{finalUserId}'");
				Console.WriteLine($"✅ UserId length: {finalUserId.Length}");
				Console.WriteLine($"✅ UserId == expected: {finalUserId == "ed198302-58d1-4c5d-b0f2-65f5a94e7667"}");

				// Verify user exists in database
				var user = await _userManager.FindByIdAsync(finalUserId);
				if (user == null)
				{
					Console.WriteLine($"❌ USER NOT FOUND IN DATABASE: '{finalUserId}'");
					return BadRequest("User account not found.");
				}

				Console.WriteLine($"✅ User verified: {user.UserName} ({user.Email})");

				var result = await _cartService.ConfirmBookingAsync(sessionId, finalUserId);

				return result ? Ok(new { message = "Booking confirmed successfully." }) : BadRequest("Booking confirmation failed");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ CONTROLLER EXCEPTION: {ex.Message}");
				Console.WriteLine($"❌ EXCEPTION DETAILS: {ex}");
				return StatusCode(500, new { message = "Internal server error", error = ex.Message });
			}
		}

		[HttpDelete("clear")]
		public async Task<IActionResult> ClearCart()
		{
			var sessionId = GetSessionId();
			await _cartService.ClearCartAsync(sessionId);
			return Ok(new { message = "Cart cleared." });
		}
	}
}
