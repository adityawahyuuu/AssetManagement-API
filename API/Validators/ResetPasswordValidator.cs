using API.Configuration;
using API.Constants;
using API.DTOs;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace API.Validators
{
    internal sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordValidator(IOptions<ValidationOptions> options)
        {
            var validationOptions = options.Value;

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ResponseMessages.EmailRequired)
                .EmailAddress().WithMessage(ResponseMessages.EmailInvalid);

            // OTP Code validation
            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required");

            // Password validation
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ResponseMessages.PasswordRequired)
                .MinimumLength(validationOptions.Password.MinLength)
                    .WithMessage(ValidationMessages.MinimumLength("password", validationOptions.Password.MinLength))
                .MaximumLength(validationOptions.Password.MaxLength)
                    .WithMessage(ValidationMessages.MaximumLength("password", validationOptions.Password.MaxLength));

            // Apply password rules based on configuration
            if (validationOptions.Password.RequireUppercase)
            {
                RuleFor(x => x.Password)
                    .Matches(@"[A-Z]+").WithMessage(ValidationMessages.PasswordRequireUppercase);
            }

            if (validationOptions.Password.RequireLowercase)
            {
                RuleFor(x => x.Password)
                    .Matches(@"[a-z]+").WithMessage(ValidationMessages.PasswordRequireLowercase);
            }

            if (validationOptions.Password.RequireDigit)
            {
                RuleFor(x => x.Password)
                    .Matches(@"[0-9]+").WithMessage(ValidationMessages.PasswordRequireDigit);
            }

            if (validationOptions.Password.RequireSpecialChar)
            {
                RuleFor(x => x.Password)
                    .Matches(validationOptions.Password.SpecialCharPattern)
                    .WithMessage(ValidationMessages.PasswordRequireSpecialChar);
            }

            // Password confirm validation
            RuleFor(x => x.PasswordConfirm)
                .Equal(x => x.Password).WithMessage(ResponseMessages.PasswordsDoNotMatch);
        }
    }
}
