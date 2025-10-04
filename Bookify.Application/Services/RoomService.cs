// Bookify-Hotel-Reservation-System-master/Bookify.Application/Services/RoomService.cs
using AutoMapper; // Added
using Bookify.Application.Business.Dtos.Rooms;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoomService> _logger;
        private readonly IMapper _mapper; // Added

        public RoomService(IUnitOfWork unitOfWork, ILogger<RoomService> logger, IMapper mapper) // Updated constructor
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(
            DateTime checkInDate,
            DateTime checkOutDate,
            CancellationToken cancellationToken = default)
        {
            var rooms = await _unitOfWork.Rooms.GetAvailableRoomsAsync(checkInDate, checkOutDate, cancellationToken);

            return rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomTypeName = r.RoomType.Name,
                Description = r.RoomType.Description,
                PricePerNight = r.RoomType.PricePerNight,
                Capacity = r.RoomType.Capacity,
                IsAvailable = r.IsAvailable
            });
        }

        public async Task<RoomSearchResultDto> SearchAvailableRoomsAsync(
            RoomSearchRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Searching rooms with filters: {@Request}", request);

                if (!request.IsValid())
                {
                    throw new ArgumentException("Invalid search parameters");
                }

                var (rooms, totalCount) = await _unitOfWork.Rooms.SearchAvailableRoomsAsync(
                    request.CheckInDate,
                    request.CheckOutDate,
                    request.RoomTypeId,
                    request.MinCapacity,
                    request.MaxPrice,
                    request.SortBy,
                    request.SortDescending,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

                var roomDtos = rooms.Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomTypeName = r.RoomType.Name,
                    Description = r.RoomType.Description,
                    PricePerNight = r.RoomType.PricePerNight,
                    Capacity = r.RoomType.Capacity,
                    IsAvailable = r.IsAvailable
                }).ToList();

                return new RoomSearchResultDto
                {
                    Rooms = roomDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching available rooms");
                throw;
            }
        }

        public async Task<IEnumerable<RoomTypeDto>> GetRoomTypesAsync(CancellationToken cancellationToken = default)
        {
            var roomTypes = await _unitOfWork.Rooms.GetRoomTypesAsync(cancellationToken);
            return roomTypes.Select(rt => new RoomTypeDto
            {
                Id = rt.Id,
                Name = rt.Name,
                Description = rt.Description,
                PricePerNight = rt.PricePerNight,
                Capacity = rt.Capacity,
                ImageUrl = rt.ImageUrl
            });
        }

        // --- New Admin Room Management Implementations ---

        public async Task<IEnumerable<RoomDto>> GetAllRoomsAdminAsync(CancellationToken cancellationToken = default)
        {
            var rooms = await _unitOfWork.Rooms.GetAllRoomsWithTypesAsync(cancellationToken);
            return _mapper.Map<IEnumerable<RoomDto>>(rooms);
        }

        public async Task<RoomDto?> GetRoomByIdAdminAsync(int id, CancellationToken cancellationToken = default)
        {
            var room = await _unitOfWork.Rooms.GetRoomByIdWithTypeAsync(id, cancellationToken);
            return _mapper.Map<RoomDto>(room);
        }

        public async Task<RoomDto> CreateRoomAsync(RoomCreateUpdateDto roomDto, CancellationToken cancellationToken = default)
        {
            var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(roomDto.RoomTypeId, cancellationToken);
            if (roomType == null)
            {
                throw new ArgumentException($"RoomType with ID {roomDto.RoomTypeId} not found.");
            }

            var room = _mapper.Map<Room>(roomDto);
            await _unitOfWork.Rooms.AddAsync(room, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with RoomType for DTO mapping
            var createdRoom = await _unitOfWork.Rooms.GetRoomByIdWithTypeAsync(room.Id, cancellationToken);
            return _mapper.Map<RoomDto>(createdRoom);
        }

        public async Task<bool> UpdateRoomAsync(int id, RoomCreateUpdateDto roomDto, CancellationToken cancellationToken = default)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id, cancellationToken);
            if (room == null) return false;

            var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(roomDto.RoomTypeId, cancellationToken);
            if (roomType == null)
            {
                throw new ArgumentException($"RoomType with ID {roomDto.RoomTypeId} not found.");
            }

            _mapper.Map(roomDto, room); // Map DTO to existing entity
            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteRoomAsync(int id, CancellationToken cancellationToken = default)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id, cancellationToken);
            if (room == null) return false;

            _unitOfWork.Rooms.Delete(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        // --- New Admin RoomType Management Implementations ---

        public async Task<RoomTypeDto?> GetRoomTypeByIdAdminAsync(int id, CancellationToken cancellationToken = default)
        {
            var roomType = await _unitOfWork.RoomTypes.GetRoomTypeByIdAsync(id, cancellationToken);
            return _mapper.Map<RoomTypeDto>(roomType);
        }

        public async Task<RoomTypeDto> CreateRoomTypeAsync(RoomTypeCreateUpdateDto roomTypeDto, CancellationToken cancellationToken = default)
        {
            var roomType = _mapper.Map<RoomType>(roomTypeDto);
            await _unitOfWork.RoomTypes.AddAsync(roomType, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<RoomTypeDto>(roomType);
        }

        public async Task<bool> UpdateRoomTypeAsync(int id, RoomTypeCreateUpdateDto roomTypeDto, CancellationToken cancellationToken = default)
        {
            var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(id, cancellationToken);
            if (roomType == null) return false;

            _mapper.Map(roomTypeDto, roomType);
            _unitOfWork.RoomTypes.Update(roomType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteRoomTypeAsync(int id, CancellationToken cancellationToken = default)
        {
            var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(id, cancellationToken);
            if (roomType == null) return false;

            _unitOfWork.RoomTypes.Delete(roomType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
