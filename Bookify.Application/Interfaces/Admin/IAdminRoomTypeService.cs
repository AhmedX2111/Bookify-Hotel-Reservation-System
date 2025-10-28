using Bookify.Application.Business.Dtos.Rooms;

namespace Bookify.Application.Business.Interfaces.Admin
{
    public interface IAdminRoomTypeService
    {
        Task<IEnumerable<RoomTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<RoomTypeDto> CreateAsync(RoomTypeCreateDto dto, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, RoomTypeUpdateDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
