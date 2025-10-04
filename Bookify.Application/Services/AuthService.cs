using Bookify.Application.Business.Dtos.Auth;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;



namespace Bookify.Infrastructure.Data.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IConfiguration _configuration;

		// List of allowed roles
		private readonly string[] _allowedRoles = { "Customer", "Admin", "Manager" };

		public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
		{
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
		{
			if (request == null)
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { "Registration request cannot be null." }
				};
			}

			// Validate role
			if (string.IsNullOrWhiteSpace(request.Role) || !_allowedRoles.Contains(request.Role))
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { $"Invalid role. Allowed roles are: {string.Join(", ", _allowedRoles)}" }
				};
			}

			// Validate email
			if (string.IsNullOrWhiteSpace(request.Email))
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { "Email is required." }
				};
			}

			// Check if user already exists
			var existingUser = await _userManager.FindByEmailAsync(request.Email);
			if (existingUser != null)
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { "This email already exists." }
				};
			}

			// Check if role exists, if not create it
			var roleExists = await _roleManager.RoleExistsAsync(request.Role);
			if (!roleExists)
			{
				// Create the role if it doesn't exist
				var roleResult = await _roleManager.CreateAsync(new IdentityRole(request.Role));
				if (!roleResult.Succeeded)
				{
					var errors = roleResult.Errors.Select(e => e.Description).ToArray();
					return new AuthResponseDto { Success = false, Errors = errors };
				}
			}

			// Create new user
			var user = new User
			{
				UserName = request.UserName ?? request.Email, // Use email as username if username not provided
				Email = request.Email,
				FirstName = request.FirstName,
				LastName = request.LastName,
				CreatedAt = DateTime.UtcNow
			};

			if (string.IsNullOrWhiteSpace(request.Password))
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { "Password is required." }
				};
			}

			var result = await _userManager.CreateAsync(user, request.Password);

			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(e => e.Description).ToArray();
				return new AuthResponseDto { Success = false, Errors = errors };
			}

			// Add user to role
			var addToRoleResult = await _userManager.AddToRoleAsync(user, request.Role);
			if (!addToRoleResult.Succeeded)
			{
				var errors = addToRoleResult.Errors.Select(e => e.Description).ToArray();
				return new AuthResponseDto { Success = false, Errors = errors };
			}

			// Generate JWT token
			var token = await GenerateJwtToken(user);
			var userRoles = await _userManager.GetRolesAsync(user);

			return new AuthResponseDto
			{
				Success = true,
				Token = token,
				UserName = user.UserName ?? string.Empty,
				Email = user.Email ?? string.Empty,
				Roles = userRoles.ToList(),
				Expiration = DateTime.UtcNow.AddHours(GetJwtExpireHours())
			};
		}

		public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
		{
			if (request == null)
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { "Login request cannot be null." }
				};
			}

			if (string.IsNullOrWhiteSpace(request.Email))
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { "Email is required." }
				};
			}

			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user == null)
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { "Invalid credentials" }
				};
			}

			if (string.IsNullOrWhiteSpace(request.Password))
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { "Password is required." }
				};
			}

			var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
			if (!isValidPassword)
			{
				return new AuthResponseDto
				{
					Success = false,
					Errors = new[] { "Invalid credentials" }
				};
			}

			// Generate JWT token
			var token = await GenerateJwtToken(user);
			var userRoles = await _userManager.GetRolesAsync(user);

			return new AuthResponseDto
			{
				Success = true,
				Token = token,
				UserName = user.UserName ?? string.Empty,
				Email = user.Email ?? string.Empty,
				Roles = userRoles.ToList(),
				Expiration = DateTime.UtcNow.AddHours(GetJwtExpireHours())
			};
		}

		private async Task<string> GenerateJwtToken(User user)
		{
			var jwtSettings = _configuration.GetSection("Jwt");
			var keyString = jwtSettings["Key"];

			if (string.IsNullOrEmpty(keyString))
			{
				throw new InvalidOperationException("JWT Key is not configured.");
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new Dictionary<string, object>
			{
				[JwtRegisteredClaimNames.Sub] = user.UserName ?? user.Email ?? string.Empty,
				[JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
				[ClaimTypes.NameIdentifier] = user.Id,
				[ClaimTypes.Email] = user.Email ?? string.Empty,
				["firstName"] = user.FirstName ?? string.Empty,
				["lastName"] = user.LastName ?? string.Empty
			};

			// Add roles to claims
			var userRoles = await _userManager.GetRolesAsync(user);
			claims[ClaimTypes.Role] = userRoles;

			var issuer = jwtSettings["Issuer"] ?? "Bookify";
			var audience = jwtSettings["Audience"] ?? "BookifyClients";
			var expires = DateTime.UtcNow.AddHours(GetJwtExpireHours());

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Issuer = issuer,
				Audience = audience,
				Claims = claims,
				Expires = expires,
				SigningCredentials = creds
			};

			var tokenHandler = new JsonWebTokenHandler();
			return tokenHandler.CreateToken(tokenDescriptor);
		}

		private double GetJwtExpireHours()
		{
			var expireHoursString = _configuration["Jwt:ExpireHours"];
			if (string.IsNullOrEmpty(expireHoursString) || !double.TryParse(expireHoursString, out double expireHours))
			{
				return 2.0; // Default to 2 hours if not configured
			}
			return expireHours;
		}
	}	
}
