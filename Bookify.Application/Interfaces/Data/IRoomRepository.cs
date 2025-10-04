// Bookify-Hotel-Reservation-System-master/Bookify.Application/Interfaces/Data/IRoomRepository.cs
using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Interfaces.Data
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, CancellationToken cancellationToken = default);

        // Add search method with filters
        Task<(IEnumerable<Room> Rooms, int TotalCount)> SearchAvailableRoomsAsync(
            DateTime checkInDate,
            DateTime checkOutDate,
            int? roomTypeId = null,
            int? minCapacity = null,
            decimal? maxPrice = null,
            string? sortBy = "PricePerNight",
            bool sortDescending = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<RoomType>> GetRoomTypesAsync(CancellationToken cancellationToken = default);

        // New: Get all rooms including unavailable ones for admin
        Task<IEnumerable<Room>> GetAllRoomsWithTypesAsync(CancellationToken cancellationToken = default);
        Task<Room?> GetRoomByIdWithTypeAsync(int id, CancellationToken cancellationToken = default);
    }
}
