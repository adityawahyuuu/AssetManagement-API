using API.Constants;
using API.DTOs;
using API.Repositories.AssetCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/asset-categories")]
    [ApiController]
    [Authorize]
    public class AssetCategoryController : BaseController
    {
        private readonly IAssetCategoryRepository _assetCategoryRepository;

        public AssetCategoryController(IAssetCategoryRepository assetCategoryRepository)
        {
            _assetCategoryRepository = assetCategoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssetCategories()
        {
            var result = await _assetCategoryRepository.GetAllAssetCategories();

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Asset categories retrieved successfully",
                data = result.Value
            });
        }
    }
}
