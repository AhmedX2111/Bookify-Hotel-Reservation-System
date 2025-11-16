using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookify.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize]
	public class ProfileController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IBookingService _bookingService;

		public ProfileController(IUserService userService, IBookingService bookingService)
		{
			_userService = userService;
			_bookingService = bookingService;
		}

		/// <summary>
		/// Gets the authenticated user's profile including personal details and booking history.
		/// </summary>
		[HttpGet]
		public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken cancellationToken)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User is not authenticated.");
			}

			try
			{
				var profile = await _userService.GetUserProfileAsync(userId, cancellationToken);
				return Ok(profile);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = "An error occurred while retrieving the profile.", Details = ex.Message });
			}
		}

		/// <summary>
		/// Gets the authenticated user's booking history.
		/// </summary>
		[HttpGet("bookings")]
		public async Task<ActionResult<IEnumerable<BookingHistoryDto>>> GetBookingHistory(CancellationToken cancellationToken)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User is not authenticated.");
			}

			try
			{
				var bookings = await _userService.GetUserBookingHistoryAsync(userId, cancellationToken);
				return Ok(bookings);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = "An error occurred while retrieving booking history.", Details = ex.Message });
			}
		}
	}
}
