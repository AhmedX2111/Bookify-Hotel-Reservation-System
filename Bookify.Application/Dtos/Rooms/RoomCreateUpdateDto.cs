// Bookify.Application/Dtos/Rooms/RoomCreateUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.Business.Dtos.Rooms
{
    public class RoomCreateUpdateDto
    {
        [Required(ErrorMessage = "Room number is required.")]
        [StringLength(10, ErrorMessage = "Room number cannot exceed 10 characters.")]
        public string RoomNumber { get; set; } = null!;

        [Required(ErrorMessage = "Room type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Room Type ID.")]
        public int RoomTypeId { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
