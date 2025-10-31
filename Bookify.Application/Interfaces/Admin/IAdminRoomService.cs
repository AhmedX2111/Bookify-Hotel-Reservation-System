using Bookify.Application.Business.Dtos.Rooms;

namespace Bookify.Application.Business.Interfaces.Admin
{
    public interface IAdminRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<RoomDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<RoomDto> CreateAsync(RoomCreateDto dto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, RoomUpdateDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
