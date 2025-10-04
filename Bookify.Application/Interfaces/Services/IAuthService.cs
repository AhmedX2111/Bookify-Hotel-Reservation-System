using Bookify.Application.Business.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Services
{
	public interface IAuthService
	{
		Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
		Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
	}
}
