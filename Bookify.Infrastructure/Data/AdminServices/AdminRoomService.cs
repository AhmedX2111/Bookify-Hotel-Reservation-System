using AutoMapper;
using Bookify.Application.Business.Dtos.Rooms;
using Bookify.Application.Business.Interfaces.Admin;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Data.Data.AdminServices
{
    public class AdminRoomService : IAdminRoomService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper; // AutoMapper or manual mapping

        public AdminRoomService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var rooms = await _uow.Rooms.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<RoomDto>>(rooms);
        }

        public async Task<RoomDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var room = await _uow.Rooms.GetByIdAsync(id, cancellationToken);
            return room == null ? null : _mapper.Map<RoomDto>(room);
        }

        public async Task<RoomDto> CreateAsync(RoomCreateDto dto, CancellationToken cancellationToken = default)
        {
            var room = _mapper.Map<Room>(dto);
            await _uow.Rooms.AddAsync(room, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return _mapper.Map<RoomDto>(room);
        }

        public async Task UpdateAsync(int id, RoomUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var room = await _uow.Rooms.GetByIdAsync(id, cancellationToken)
                       ?? throw new KeyNotFoundException("Room not found");

            _mapper.Map(dto, room);
            _uow.Rooms.Update(room);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var room = await _uow.Rooms.GetByIdAsync(id, cancellationToken)
                       ?? throw new KeyNotFoundException("Room not found");

            // business rule: don't delete if there are future bookings
            var hasFutureBookings = await _uow.Bookings.GetBookingsPagedAsync(1, 1, null, cancellationToken)
                .ContinueWith(t => t.Result.Any(b => b.RoomId == id && b.CheckOutDate >= DateTime.UtcNow), cancellationToken);

            if (hasFutureBookings)
                throw new InvalidOperationException("Cannot delete room with future bookings.");

            _uow.Rooms.Delete(room);
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
