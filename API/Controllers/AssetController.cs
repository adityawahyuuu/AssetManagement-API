using API.Constants;
using API.DTOs;
using API.Repositories.Asset;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/assets")]
    [ApiController]
    [Authorize]
    public class AssetController : BaseController
    {
        private readonly IAssetRepository _assetRepository;

        public AssetController(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
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

        [HttpGet]
        public async Task<IActionResult> GetAllAssetsPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

            var result = await _assetRepository.GetAllAssetsPaginated(userId, page, pageSize);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Assets retrieved successfully",
                data = result.Value.Data,
                page = result.Value.Page,
                pageSize = result.Value.PageSize,
                totalCount = result.Value.TotalCount,
                totalPages = result.Value.TotalPages,
                hasPreviousPage = result.Value.HasPreviousPage,
                hasNextPage = result.Value.HasNextPage
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddAsset([FromBody] AddAssetDto addAssetDto)
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

            var result = await _assetRepository.AddAsset(userId, addAssetDto);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Asset added successfully",
                data = result.Value
            });
        }

        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetAssetsByRoomId(int roomId)
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

            var result = await _assetRepository.GetAssetsByRoomId(roomId, userId);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Assets retrieved successfully",
                data = result.Value
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssetById(int id)
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

            var result = await _assetRepository.GetAssetById(id, userId);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Asset retrieved successfully",
                data = result.Value
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] UpdateAssetDto updateAssetDto)
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

            var result = await _assetRepository.UpdateAsset(id, userId, updateAssetDto);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Asset updated successfully",
                data = result.Value
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
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

            var result = await _assetRepository.DeleteAsset(id, userId);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Asset deleted successfully",
                data = new { }
            });
        }
    }
}
