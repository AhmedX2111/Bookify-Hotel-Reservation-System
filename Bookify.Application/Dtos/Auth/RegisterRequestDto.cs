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

		[Required]
		public string UserName { get; set; } = string.Empty;

		[Required]
		[MinLength(6)]
		public string Password { get; set; } = string.Empty;

		[Required]
		public string Role { get; set; } = string.Empty;
	}
}
