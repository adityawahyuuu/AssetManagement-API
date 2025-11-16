using API.Common;
using API.Data;
using API.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories.AssetCategory
{
    public class AssetCategoryRepository : IAssetCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AssetCategoryRepository> _logger;

        public AssetCategoryRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<AssetCategoryRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<AssetCategoryResponseDto>>> GetAllAssetCategories()
        {
            try
            {
                _logger.LogInformation("Fetching all asset categories");

                var categories = await _context.AssetCategories
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var categoryDtos = _mapper.Map<List<AssetCategoryResponseDto>>(categories);

                _logger.LogInformation("Successfully fetched {Count} asset categories", categories.Count);

                return Result<List<AssetCategoryResponseDto>>.Success(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching asset categories: {Message}", ex.Message);
                return Result.Failure<List<AssetCategoryResponseDto>>("An error occurred while fetching asset categories");
            }
        }
    }
}
