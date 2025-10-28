namespace Bookify.Application.Business.Dtos.Rooms
{
    public class RoomTypeCreateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? ImageUrl { get; set; }
    }
}
