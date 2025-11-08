using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Auth
{
	public class RegisterRequestDto
	{
		[Required]
		public string FirstName { get; set; } = string.Empty;

		[Required]
		public string LastName { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	public string? UserName { get; set; }  // Optional - will use Email if not provided

	[Required]
	[MinLength(6)]
	public string Password { get; set; } = string.Empty;

	public string? ConfirmPassword { get; set; }  // For validation

	public string Role { get; set; } = "Customer";  // Default to Customer role
	}
}
