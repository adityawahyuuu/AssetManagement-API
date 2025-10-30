namespace API.Constants
{
    public static class ResponseMessages
    {
        // Response Types
        public const string Success = "Success";
        public const string Failed = "Failed";

        // User Registration
        public const string RegistrationSuccessful = "Registration successful. Please check your email to confirm your account.";
        public const string EmailAlreadyRegistered = "Email already registered";
        public const string FailedToMapUserData = "Failed to map user data";
        public const string FailedToCreateUser = "Failed to create user. Please try again later.";

        // Validation Messages
        public const string EmailRequired = "Email is required";
        public const string EmailInvalid = "Email address is not valid";
        public const string UsernameRequired = "Username is required";
        public const string PasswordRequired = "Password is required";
        public const string PasswordsDoNotMatch = "Passwords do not match";

        // Global Exception Handler
        public const string UnhandledErrorOccurred = "An error occurred while processing your request.";
        public const string ValidationFailed = "Validation failed";
        public const string InvalidArgument = "Invalid argument";
        public const string ResourceNotFound = "Resource not found";
        public const string UnauthorizedAccess = "Unauthorized access";
        public const string InvalidOperation = "Invalid operation";
        public const string InternalServerError = "Internal server error";
    }

    public static class ValidationMessages
    {
        public static string MinimumLength(string field, int length)
            => $"Your {field} length must be at least {length}.";

        public static string MaximumLength(string field, int length)
            => $"Your {field} length must not exceed {length}.";

        public const string PasswordRequireUppercase = "Your password must contain at least one uppercase letter.";
        public const string PasswordRequireLowercase = "Your password must contain at least one lowercase letter.";
        public const string PasswordRequireDigit = "Your password must contain at least one number.";
        public const string PasswordRequireSpecialChar = "Your password must contain at least one (!? *.).";
    }
}
