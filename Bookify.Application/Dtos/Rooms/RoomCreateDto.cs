namespace Bookify.Application.Business.Dtos.Rooms
{
    public class RoomCreateDto
    {
        public string RoomNumber { get; set; } = null!;
        public string RoomTypeName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
    }
}
