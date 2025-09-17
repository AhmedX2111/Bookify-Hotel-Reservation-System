using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Dtos.Auth
{
	public class AuthResponseDto
	{
		public bool Success { get; set; }
		public string[] Errors { get; set; } = Array.Empty<string>();
		public string Token { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public List<string> Roles { get; set; } = new List<string>();
		public DateTime Expiration { get; set; }
	}
}
