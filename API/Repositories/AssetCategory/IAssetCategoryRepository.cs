using API.Common;
using API.DTOs;

namespace API.Repositories.AssetCategory
{
    public interface IAssetCategoryRepository
    {
        Task<Result<List<AssetCategoryResponseDto>>> GetAllAssetCategories();
    }
}
