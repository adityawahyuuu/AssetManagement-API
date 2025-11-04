using API.Constants;
using API.DTOs;
using FluentValidation;

namespace API.Validators
{
    internal sealed class AddAssetValidator : AbstractValidator<AddAssetDto>
    {
        public AddAssetValidator()
        {
            RuleFor(x => x.RoomId)
                .GreaterThan(0).WithMessage("Room ID is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Asset name is required")
                .MaximumLength(255).WithMessage("Asset name must not exceed 255 characters");

            RuleFor(x => x.Category)
                .Must(category => string.IsNullOrEmpty(category) || AssetConstants.Categories.AllowedValues.Contains(category))
                .WithMessage($"Category must be one of: {string.Join(", ", AssetConstants.Categories.AllowedValues)}");

            RuleFor(x => x.LengthCm)
                .GreaterThan(0).WithMessage("Length must be greater than 0");

            RuleFor(x => x.WidthCm)
                .GreaterThan(0).WithMessage("Width must be greater than 0");

            RuleFor(x => x.HeightCm)
                .GreaterThan(0).WithMessage("Height must be greater than 0");

            RuleFor(x => x.ClearanceFrontCm)
                .GreaterThanOrEqualTo(0).When(x => x.ClearanceFrontCm.HasValue)
                .WithMessage("Clearance front must be 0 or greater");

            RuleFor(x => x.ClearanceSidesCm)
                .GreaterThanOrEqualTo(0).When(x => x.ClearanceSidesCm.HasValue)
                .WithMessage("Clearance sides must be 0 or greater");

            RuleFor(x => x.ClearanceBackCm)
                .GreaterThanOrEqualTo(0).When(x => x.ClearanceBackCm.HasValue)
                .WithMessage("Clearance back must be 0 or greater");

            RuleFor(x => x.FunctionZone)
                .Must(zone => string.IsNullOrEmpty(zone) || AssetConstants.FunctionZones.AllowedValues.Contains(zone))
                .WithMessage($"Function zone must be one of: {string.Join(", ", AssetConstants.FunctionZones.AllowedValues)}");

            RuleFor(x => x.Condition)
                .Must(condition => string.IsNullOrEmpty(condition) || AssetConstants.Conditions.AllowedValues.Contains(condition))
                .WithMessage($"Condition must be one of: {string.Join(", ", AssetConstants.Conditions.AllowedValues)}");

            RuleFor(x => x.PurchasePrice)
                .GreaterThanOrEqualTo(0).When(x => x.PurchasePrice.HasValue)
                .WithMessage("Purchase price must be 0 or greater");
        }
    }
}
