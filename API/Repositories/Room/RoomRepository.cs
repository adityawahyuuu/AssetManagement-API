using API.Common;
using API.Constants;
using API.Data;
using API.DTOs;
using API.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories.Room
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<RoomRepository> _logger;

        public RoomRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<RoomRepository> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<RoomResponseDto>> AddRoom(int userId, AddRoomDto addRoomDto)
        {
            try
            {
                // Verify user exists
                var userExists = await _dbContext.user_logins
                    .AnyAsync(u => u.userid == userId);

                if (!userExists)
                {
                    return Result.Failure<RoomResponseDto>("User not found");
                }

                // Create new room
                var room = new API.Models.Room
                {
                    UserId = userId,
                    Name = addRoomDto.Name,
                    LengthM = addRoomDto.LengthM,
                    WidthM = addRoomDto.WidthM,
                    DoorPosition = addRoomDto.DoorPosition,
                    DoorWidthCm = addRoomDto.DoorWidthCm,
                    WindowPosition = addRoomDto.WindowPosition,
                    WindowWidthCm = addRoomDto.WindowWidthCm,
                    PowerOutletPositions = addRoomDto.PowerOutletPositions,
                    PhotoUrl = addRoomDto.PhotoUrl,
                    Notes = addRoomDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Rooms.Add(room);
                await _dbContext.SaveChangesAsync();

                var roomResponse = _mapper.Map<RoomResponseDto>(room);
                return Result.Success(roomResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding room for user {UserId}", userId);
                return Result.Failure<RoomResponseDto>("Failed to add room");
            }
        }

        public async Task<Result<List<RoomResponseDto>>> GetAllUserRooms(int userId)
        {
            try
            {
                var rooms = await _dbContext.Rooms
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                var roomResponses = _mapper.Map<List<RoomResponseDto>>(rooms);
                return Result.Success(roomResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms for user {UserId}", userId);
                return Result.Failure<List<RoomResponseDto>>("Failed to retrieve rooms");
            }
        }

        public async Task<Result<RoomResponseDto>> GetRoomById(int roomId, int userId)
        {
            try
            {
                var room = await _dbContext.Rooms
                    .FirstOrDefaultAsync(r => r.Id == roomId && r.UserId == userId);

                if (room == null)
                {
                    return Result.Failure<RoomResponseDto>("Room not found");
                }

                var roomResponse = _mapper.Map<RoomResponseDto>(room);
                return Result.Success(roomResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room {RoomId} for user {UserId}", roomId, userId);
                return Result.Failure<RoomResponseDto>("Failed to retrieve room");
            }
        }

        public async Task<Result<RoomResponseDto>> UpdateRoom(int roomId, int userId, UpdateRoomDto updateRoomDto)
        {
            try
            {
                var room = await _dbContext.Rooms
                    .FirstOrDefaultAsync(r => r.Id == roomId && r.UserId == userId);

                if (room == null)
                {
                    return Result.Failure<RoomResponseDto>("Room not found");
                }

                // Update only provided fields
                if (updateRoomDto.Name != null)
                    room.Name = updateRoomDto.Name;

                if (updateRoomDto.LengthM.HasValue)
                    room.LengthM = updateRoomDto.LengthM.Value;

                if (updateRoomDto.WidthM.HasValue)
                    room.WidthM = updateRoomDto.WidthM.Value;

                if (updateRoomDto.DoorPosition != null)
                    room.DoorPosition = updateRoomDto.DoorPosition;

                if (updateRoomDto.DoorWidthCm.HasValue)
                    room.DoorWidthCm = updateRoomDto.DoorWidthCm;

                if (updateRoomDto.WindowPosition != null)
                    room.WindowPosition = updateRoomDto.WindowPosition;

                if (updateRoomDto.WindowWidthCm.HasValue)
                    room.WindowWidthCm = updateRoomDto.WindowWidthCm;

                if (updateRoomDto.PowerOutletPositions != null)
                    room.PowerOutletPositions = updateRoomDto.PowerOutletPositions;

                if (updateRoomDto.PhotoUrl != null)
                    room.PhotoUrl = updateRoomDto.PhotoUrl;

                if (updateRoomDto.Notes != null)
                    room.Notes = updateRoomDto.Notes;

                room.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                var roomResponse = _mapper.Map<RoomResponseDto>(room);
                return Result.Success(roomResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {RoomId} for user {UserId}", roomId, userId);
                return Result.Failure<RoomResponseDto>("Failed to update room");
            }
        }

        public async Task<Result> DeleteRoom(int roomId, int userId)
        {
            try
            {
                var room = await _dbContext.Rooms
                    .FirstOrDefaultAsync(r => r.Id == roomId && r.UserId == userId);

                if (room == null)
                {
                    return Result.Failure("Room not found");
                }

                // Assets will be deleted automatically due to CASCADE delete
                _dbContext.Rooms.Remove(room);
                await _dbContext.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room {RoomId} for user {UserId}", roomId, userId);
                return Result.Failure("Failed to delete room");
            }
        }
    }
}
