using Bookify.Application.Business.Dtos.Rooms;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Services
{
	public class RoomService : IRoomService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<RoomService> _logger;

		public RoomService(IUnitOfWork unitOfWork, ILogger<RoomService> logger)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
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
	}
}
