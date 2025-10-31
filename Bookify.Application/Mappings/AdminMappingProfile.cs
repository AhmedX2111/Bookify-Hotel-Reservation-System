using Bookify.Application.Business.Dtos.Bookings;
using Bookify.Application.Business.Dtos.Rooms;
using Bookify.Domain.Entities;

namespace Bookify.Application.Business.Mappings
{
    public class AdminMappingProfile : AutoMapper.Profile
    {
        public AdminMappingProfile()
        {
            CreateMap<Room, RoomDto>()
                .ForMember(d => d.RoomTypeName, opt => opt.MapFrom(s => s.RoomType.Name));
            CreateMap<RoomCreateDto, Room>();
            CreateMap<RoomUpdateDto, Room>();

            CreateMap<RoomType, RoomTypeDto>();
            CreateMap<RoomTypeCreateDto, RoomType>();
            CreateMap<RoomTypeUpdateDto, RoomType>();

            CreateMap<Booking, BookingDto>();
        }
    }
}
