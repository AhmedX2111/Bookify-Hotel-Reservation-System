// Bookify-Hotel-Reservation-System-master/Bookify.Application/Interfaces/Services/IRoomService.cs
using Bookify.Application.Business.Dtos.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Services
{
    public interface IRoomService
    {
        /// <summary>
        /// Retrieves available rooms for a given date range
        /// </summary>
        Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(
            DateTime checkInDate,
            DateTime checkOutDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches available rooms with filters and pagination
        /// </summary>
        Task<RoomSearchResultDto> SearchAvailableRoomsAsync(
            RoomSearchRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all room types
        /// </summary>
        Task<IEnumerable<RoomTypeDto>> GetRoomTypesAsync(CancellationToken cancellationToken = default);

        // New: Admin Room Management
        Task<IEnumerable<RoomDto>> GetAllRoomsAdminAsync(CancellationToken cancellationToken = default);
        Task<RoomDto?> GetRoomByIdAdminAsync(int id, CancellationToken cancellationToken = default);
        Task<RoomDto> CreateRoomAsync(RoomCreateUpdateDto roomDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateRoomAsync(int id, RoomCreateUpdateDto roomDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteRoomAsync(int id, CancellationToken cancellationToken = default);

        // New: Admin RoomType Management
        Task<RoomTypeDto?> GetRoomTypeByIdAdminAsync(int id, CancellationToken cancellationToken = default);
        Task<RoomTypeDto> CreateRoomTypeAsync(RoomTypeCreateUpdateDto roomTypeDto, CancellationToken cancellationToken = default);
        Task<bool> UpdateRoomTypeAsync(int id, RoomTypeCreateUpdateDto roomTypeDto, CancellationToken cancellationToken = default);
        Task<bool> DeleteRoomTypeAsync(int id, CancellationToken cancellationToken = default);
    }
}
