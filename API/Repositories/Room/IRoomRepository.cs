using API.Common;
using API.DTOs;

namespace API.Repositories.Room
{
    public interface IRoomRepository
    {
        Task<Result<RoomResponseDto>> AddRoom(int userId, AddRoomDto addRoomDto);
        Task<Result<List<RoomResponseDto>>> GetAllUserRooms(int userId);
        Task<Result<RoomResponseDto>> GetRoomById(int roomId, int userId);
        Task<Result<RoomResponseDto>> UpdateRoom(int roomId, int userId, UpdateRoomDto updateRoomDto);
        Task<Result> DeleteRoom(int roomId, int userId);
    }
}
