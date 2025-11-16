using API.Common;
using API.Constants;
using API.Data;
using API.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Repositories.Asset
{
    public class AssetRepository : IAssetRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AssetRepository> _logger;

        public AssetRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<AssetRepository> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<AssetResponseDto>> AddAsset(int userId, AddAssetDto addAssetDto)
        {
            try
            {
                // Verify room exists and belongs to the user (also validates user via FK)
                var room = await _dbContext.Rooms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == addAssetDto.RoomId && r.UserId == userId);

                if (room == null)
                {
                    return Result.Failure<AssetResponseDto>("Room not found or does not belong to the user");
                }

                // Create new asset
                var asset = new API.Models.Asset
                {
                    RoomId = addAssetDto.RoomId,
                    UserId = userId,
                    Name = addAssetDto.Name,
                    Category = addAssetDto.Category,
                    PhotoUrl = addAssetDto.PhotoUrl,
                    LengthCm = addAssetDto.LengthCm,
                    WidthCm = addAssetDto.WidthCm,
                    HeightCm = addAssetDto.HeightCm,
                    ClearanceFrontCm = addAssetDto.ClearanceFrontCm ?? 0,
                    ClearanceSidesCm = addAssetDto.ClearanceSidesCm ?? 0,
                    ClearanceBackCm = addAssetDto.ClearanceBackCm ?? 0,
                    FunctionZone = addAssetDto.FunctionZone,
                    MustBeNearWall = addAssetDto.MustBeNearWall ?? false,
                    MustBeNearWindow = addAssetDto.MustBeNearWindow ?? false,
                    MustBeNearOutlet = addAssetDto.MustBeNearOutlet ?? false,
                    CanRotate = addAssetDto.CanRotate ?? true,
                    CannotAdjacentTo = addAssetDto.CannotAdjacentTo,
                    PurchaseDate = addAssetDto.PurchaseDate,
                    PurchasePrice = addAssetDto.PurchasePrice,
                    Condition = addAssetDto.Condition,
                    Notes = addAssetDto.Notes,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Check if we're using in-memory database (transactions not supported)
                var isInMemory = _dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

                if (isInMemory)
                {
                    // In-memory database: no transaction support
                    _dbContext.Assets.Add(asset);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Real database: use transaction with execution strategy for retry support
                    var strategy = _dbContext.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _dbContext.Database.BeginTransactionAsync();
                        try
                        {
                            _dbContext.Assets.Add(asset);
                            await _dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });
                }

                var assetResponse = _mapper.Map<AssetResponseDto>(asset);
                return Result.Success(assetResponse);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx)
            {
                _logger.LogError(ex, "Database constraint violation when adding asset for user {UserId}", userId);

                var errorMessages = new List<string>();

                // Handle specific constraint violations
                if (postgresEx.ConstraintName == "assets_category_check")
                {
                    errorMessages.Add($"Invalid category. Allowed values: {string.Join(", ", AssetConstants.Categories.AllowedValues)}");
                }
                else if (postgresEx.ConstraintName == "assets_function_zone_check")
                {
                    errorMessages.Add($"Invalid function zone. Allowed values: {string.Join(", ", AssetConstants.FunctionZones.AllowedValues)}");
                }
                else if (postgresEx.ConstraintName == "assets_condition_check")
                {
                    errorMessages.Add($"Invalid condition. Allowed values: {string.Join(", ", AssetConstants.Conditions.AllowedValues)}");
                }
                else if (postgresEx.ConstraintName?.StartsWith("fk_") == true)
                {
                    errorMessages.Add("Referenced resource not found");
                }
                else
                {
                    errorMessages.Add($"Database constraint violation: {postgresEx.MessageText}");
                }

                return Result.Failure<AssetResponseDto>(errorMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding asset for user {UserId}", userId);
                return Result.Failure<AssetResponseDto>("Failed to add asset. Please check your input and try again.");
            }
        }

        public async Task<Result<List<AssetResponseDto>>> GetAssetsByRoomId(int roomId, int userId)
        {
            try
            {
                // Verify room exists and belongs to the user
                var roomExists = await _dbContext.Rooms
                    .AsNoTracking()
                    .AnyAsync(r => r.Id == roomId && r.UserId == userId);

                if (!roomExists)
                {
                    return Result.Failure<List<AssetResponseDto>>("Room not found or does not belong to the user");
                }

                var assets = await _dbContext.Assets
                    .AsNoTracking()
                    .Where(a => a.RoomId == roomId && a.UserId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var assetResponses = _mapper.Map<List<AssetResponseDto>>(assets);
                return Result.Success(assetResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assets for room {RoomId} and user {UserId}", roomId, userId);
                return Result.Failure<List<AssetResponseDto>>("Failed to retrieve assets");
            }
        }

        public async Task<Result<AssetResponseDto>> GetAssetById(int assetId, int userId)
        {
            try
            {
                var asset = await _dbContext.Assets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == assetId && a.UserId == userId);

                if (asset == null)
                {
                    return Result.Failure<AssetResponseDto>("Asset not found");
                }

                var assetResponse = _mapper.Map<AssetResponseDto>(asset);
                return Result.Success(assetResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset {AssetId} for user {UserId}", assetId, userId);
                return Result.Failure<AssetResponseDto>("Failed to retrieve asset");
            }
        }

        public async Task<Result<AssetResponseDto>> UpdateAsset(int assetId, int userId, UpdateAssetDto updateAssetDto)
        {
            try
            {
                var asset = await _dbContext.Assets
                    .AsTracking()
                    .FirstOrDefaultAsync(a => a.Id == assetId && a.UserId == userId);

                if (asset == null)
                {
                    return Result.Failure<AssetResponseDto>("Asset not found");
                }

                // Update only provided fields
                if (updateAssetDto.Name != null)
                    asset.Name = updateAssetDto.Name;

                if (updateAssetDto.Category != null)
                    asset.Category = updateAssetDto.Category;

                if (updateAssetDto.PhotoUrl != null)
                    asset.PhotoUrl = updateAssetDto.PhotoUrl;

                if (updateAssetDto.LengthCm.HasValue)
                    asset.LengthCm = updateAssetDto.LengthCm.Value;

                if (updateAssetDto.WidthCm.HasValue)
                    asset.WidthCm = updateAssetDto.WidthCm.Value;

                if (updateAssetDto.HeightCm.HasValue)
                    asset.HeightCm = updateAssetDto.HeightCm.Value;

                if (updateAssetDto.ClearanceFrontCm.HasValue)
                    asset.ClearanceFrontCm = updateAssetDto.ClearanceFrontCm.Value;

                if (updateAssetDto.ClearanceSidesCm.HasValue)
                    asset.ClearanceSidesCm = updateAssetDto.ClearanceSidesCm.Value;

                if (updateAssetDto.ClearanceBackCm.HasValue)
                    asset.ClearanceBackCm = updateAssetDto.ClearanceBackCm.Value;

                if (updateAssetDto.FunctionZone != null)
                    asset.FunctionZone = updateAssetDto.FunctionZone;

                if (updateAssetDto.MustBeNearWall.HasValue)
                    asset.MustBeNearWall = updateAssetDto.MustBeNearWall.Value;

                if (updateAssetDto.MustBeNearWindow.HasValue)
                    asset.MustBeNearWindow = updateAssetDto.MustBeNearWindow.Value;

                if (updateAssetDto.MustBeNearOutlet.HasValue)
                    asset.MustBeNearOutlet = updateAssetDto.MustBeNearOutlet.Value;

                if (updateAssetDto.CanRotate.HasValue)
                    asset.CanRotate = updateAssetDto.CanRotate.Value;

                if (updateAssetDto.CannotAdjacentTo != null)
                    asset.CannotAdjacentTo = updateAssetDto.CannotAdjacentTo;

                if (updateAssetDto.PurchaseDate.HasValue)
                    asset.PurchaseDate = updateAssetDto.PurchaseDate;

                if (updateAssetDto.PurchasePrice.HasValue)
                    asset.PurchasePrice = updateAssetDto.PurchasePrice;

                if (updateAssetDto.Condition != null)
                    asset.Condition = updateAssetDto.Condition;

                if (updateAssetDto.Notes != null)
                    asset.Notes = updateAssetDto.Notes;

                asset.UpdatedAt = DateTime.Now;

                // Check if we're using in-memory database (transactions not supported)
                var isInMemory = _dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

                if (isInMemory)
                {
                    // In-memory database: no transaction support
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Real database: use transaction with execution strategy for retry support
                    var strategy = _dbContext.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _dbContext.Database.BeginTransactionAsync();
                        try
                        {
                            await _dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });
                }

                var assetResponse = _mapper.Map<AssetResponseDto>(asset);
                return Result.Success(assetResponse);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx)
            {
                _logger.LogError(ex, "Database constraint violation when updating asset {AssetId} for user {UserId}", assetId, userId);

                var errorMessages = new List<string>();

                // Handle specific constraint violations
                if (postgresEx.ConstraintName == "assets_category_check")
                {
                    errorMessages.Add($"Invalid category. Allowed values: {string.Join(", ", AssetConstants.Categories.AllowedValues)}");
                }
                else if (postgresEx.ConstraintName == "assets_function_zone_check")
                {
                    errorMessages.Add($"Invalid function zone. Allowed values: {string.Join(", ", AssetConstants.FunctionZones.AllowedValues)}");
                }
                else if (postgresEx.ConstraintName == "assets_condition_check")
                {
                    errorMessages.Add($"Invalid condition. Allowed values: {string.Join(", ", AssetConstants.Conditions.AllowedValues)}");
                }
                else if (postgresEx.ConstraintName?.StartsWith("fk_") == true)
                {
                    errorMessages.Add("Referenced resource not found");
                }
                else
                {
                    errorMessages.Add($"Database constraint violation: {postgresEx.MessageText}");
                }

                return Result.Failure<AssetResponseDto>(errorMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset {AssetId} for user {UserId}", assetId, userId);
                return Result.Failure<AssetResponseDto>("Failed to update asset. Please check your input and try again.");
            }
        }

        public async Task<Result> DeleteAsset(int assetId, int userId)
        {
            try
            {
                var asset = await _dbContext.Assets
                    .AsTracking()
                    .FirstOrDefaultAsync(a => a.Id == assetId && a.UserId == userId);

                if (asset == null)
                {
                    return Result.Failure("Asset not found");
                }

                // Check if we're using in-memory database (transactions not supported)
                var isInMemory = _dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

                if (isInMemory)
                {
                    // In-memory database: no transaction support
                    _dbContext.Assets.Remove(asset);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Real database: use transaction with execution strategy for retry support
                    var strategy = _dbContext.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _dbContext.Database.BeginTransactionAsync();
                        try
                        {
                            _dbContext.Assets.Remove(asset);
                            await _dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });
                }

                return Result.Success();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx)
            {
                _logger.LogError(ex, "Database constraint violation when deleting asset {AssetId} for user {UserId}", assetId, userId);

                var errorMessages = new List<string>();

                // Handle specific constraint violations
                if (postgresEx.ConstraintName?.StartsWith("fk_") == true)
                {
                    errorMessages.Add("Cannot delete asset because it is referenced by other resources");
                }
                else
                {
                    errorMessages.Add($"Database constraint violation: {postgresEx.MessageText}");
                }

                return Result.Failure(errorMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset {AssetId} for user {UserId}", assetId, userId);
                return Result.Failure("Failed to delete asset. Please try again.");
            }
        }

        public async Task<Result<PaginatedResponse<AssetResponseDto>>> GetAllAssetsPaginated(int userId, int page, int pageSize)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Max page size

                // Get total count
                var totalCount = await _dbContext.Assets
                    .AsNoTracking()
                    .Where(a => a.UserId == userId)
                    .CountAsync();

                // Calculate pagination
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var skip = (page - 1) * pageSize;

                // Get paginated data
                var assets = await _dbContext.Assets
                    .AsNoTracking()
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                var assetResponses = _mapper.Map<List<AssetResponseDto>>(assets);

                var paginatedResponse = new PaginatedResponse<AssetResponseDto>
                {
                    Data = assetResponses,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = page > 1,
                    HasNextPage = page < totalPages
                };

                return Result.Success(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated assets for user {UserId}", userId);
                return Result.Failure<PaginatedResponse<AssetResponseDto>>("Failed to retrieve assets");
            }
        }
    }
}
