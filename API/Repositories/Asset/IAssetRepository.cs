using API.Common;
using API.DTOs;

namespace API.Repositories.Asset
{
    public interface IAssetRepository
    {
        Task<Result<AssetResponseDto>> AddAsset(int userId, AddAssetDto addAssetDto);
        Task<Result<List<AssetResponseDto>>> GetAssetsByRoomId(int roomId, int userId);
        Task<Result<AssetResponseDto>> GetAssetById(int assetId, int userId);
        Task<Result<AssetResponseDto>> UpdateAsset(int assetId, int userId, UpdateAssetDto updateAssetDto);
        Task<Result> DeleteAsset(int assetId, int userId);
        Task<Result<PaginatedResponse<AssetResponseDto>>> GetAllAssetsPaginated(int userId, int page, int pageSize);
    }
}
