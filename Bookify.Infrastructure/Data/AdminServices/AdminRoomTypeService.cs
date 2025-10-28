using AutoMapper;
using Bookify.Application.Business.Dtos.Rooms;
using Bookify.Application.Business.Interfaces.Admin;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Data.Data.AdminServices
{
    public class AdminRoomTypeService : IAdminRoomTypeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AdminRoomTypeService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoomTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var roomTypes = await _uow.RoomTypes.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<RoomTypeDto>>(roomTypes);
        }

        public async Task<RoomTypeDto> CreateAsync(RoomTypeCreateDto dto, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<RoomType>(dto);
            await _uow.RoomTypes.AddAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return _mapper.Map<RoomTypeDto>(entity);
        }

        public async Task UpdateAsync(int id, RoomTypeUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var roomType = await _uow.RoomTypes.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException("Room type not found.");

            _mapper.Map(dto, roomType);
            _uow.RoomTypes.Update(roomType);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var roomType = await _uow.RoomTypes.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException("Room type not found.");

            // Prevent deleting if rooms exist under this type
            var rooms = await _uow.Rooms.GetAllAsync(cancellationToken);
            if (rooms.Any(r => r.RoomTypeId == id))
                throw new InvalidOperationException("Cannot delete RoomType while Rooms are assigned to it.");

            _uow.RoomTypes.Delete(roomType);
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
