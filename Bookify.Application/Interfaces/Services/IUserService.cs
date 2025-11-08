using Bookify.Application.Business.Dtos.Bookings;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<BookingHistoryDto>> GetUserBookingHistoryAsync(string userId, CancellationToken cancellationToken = default);
    }
}

