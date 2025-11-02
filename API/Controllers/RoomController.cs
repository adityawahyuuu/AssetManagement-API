using API.Constants;
using API.DTOs;
using API.Repositories.Room;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    [Authorize]
    [EnableCors("AllowLocal")]  // ← Add this
    public class RoomController : BaseController
    {
        private readonly IRoomRepository _roomRepository;

        public RoomController(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return 0;
            }
            return userId;
        }

        [HttpPost]
        public async Task<IActionResult> AddRoom([FromBody] AddRoomDto addRoomDto)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
            {
                return Unauthorized(new
                {
                    type = ResponseMessages.Failed,
                    message = "Invalid token"
                });
            }

            var result = await _roomRepository.AddRoom(userId, addRoomDto);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Room added successfully",
                data = result.Value
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserRooms()
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
            {
                return Unauthorized(new
                {
                    type = ResponseMessages.Failed,
                    message = "Invalid token"
                });
            }

            var result = await _roomRepository.GetAllUserRooms(userId);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Rooms retrieved successfully",
                data = result.Value
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
            {
                return Unauthorized(new
                {
                    type = ResponseMessages.Failed,
                    message = "Invalid token"
                });
            }

            var result = await _roomRepository.GetRoomById(id, userId);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Room retrieved successfully",
                data = result.Value
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDto updateRoomDto)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
            {
                return Unauthorized(new
                {
                    type = ResponseMessages.Failed,
                    message = "Invalid token"
                });
            }

            var result = await _roomRepository.UpdateRoom(id, userId, updateRoomDto);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Room updated successfully",
                data = result.Value
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
            {
                return Unauthorized(new
                {
                    type = ResponseMessages.Failed,
                    message = "Invalid token"
                });
            }

            var result = await _roomRepository.DeleteRoom(id, userId);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Room and all its assets deleted successfully",
                data = new { }
            });
        }
    }
}
