// Bookify.Application/Dtos/Rooms/RoomTypeCreateUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.Business.Dtos.Rooms
{
    public class RoomTypeCreateUpdateDto
    {
        [Required(ErrorMessage = "Room type name is required.")]
        [StringLength(100, ErrorMessage = "Room type name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Price per night is required.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price per night must be greater than 0.")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
        public int Capacity { get; set; }

        public string? ImageUrl { get; set; }
    }
}
