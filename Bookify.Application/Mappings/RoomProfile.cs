// Bookify-Hotel-Reservation-System-master/Bookify.Application/Mappings/RoomProfile.cs
using AutoMapper;
using Bookify.Application.Business.Dtos.Rooms;
using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Business.Mappings
{
    public class RoomProfile : Profile
    {
        public RoomProfile()
        {
            CreateMap<Room, RoomDto>()
                .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.RoomType.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.RoomType.Description))
                .ForMember(dest => dest.PricePerNight, opt => opt.MapFrom(src => src.RoomType.PricePerNight))
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.RoomType.Capacity));

            // New mappings for admin CRUD
            CreateMap<RoomCreateUpdateDto, Room>();
            CreateMap<RoomTypeCreateUpdateDto, RoomType>();
            CreateMap<RoomType, RoomTypeDto>(); // Ensure RoomType to RoomTypeDto mapping exists
        }
    }
}
