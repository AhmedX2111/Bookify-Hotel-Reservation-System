using Bookify.Application.Business.Dtos.Auth;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bookify.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("register")]
		public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
		{
			var result = await _authService.RegisterAsync(request);

			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

		[HttpPost("login")]
		public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
		{
			var result = await _authService.LoginAsync(request);

			if (!result.Success)
			{
				return Unauthorized(result);
			}

			return Ok(result);
		}
	}
}


