using Bookify.Application.Business.Dtos.Rooms;
using Bookify.Application.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class AdminRoomsController : ControllerBase
    {
        private readonly IAdminRoomService _roomService;

        public AdminRoomsController(IAdminRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var rooms = await _roomService.GetAllAsync(cancellationToken);
            return Ok(rooms);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var room = await _roomService.GetByIdAsync(id, cancellationToken);
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoomCreateDto dto, CancellationToken cancellationToken)
        {
            var created = await _roomService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoomUpdateDto dto, CancellationToken cancellationToken)
        {
            await _roomService.UpdateAsync(id, dto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _roomService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
