namespace API.Constants
{
    public static class OtpMessages
    {
        // OTP Generation
        public const string OtpSentSuccessfully = "OTP has been sent to your email. Please check your inbox.";
        public const string FailedToGenerateOtp = "Failed to generate OTP. Please try again.";
        public const string FailedToSendOtpEmail = "Failed to send OTP email. Please try again.";

        // OTP Verification
        public const string OtpVerifiedSuccessfully = "Email verified successfully. Your account has been created.";
        public const string OtpNotFound = "OTP not found or already verified.";
        public const string OtpExpired = "OTP has expired. Please request a new one.";
        public const string OtpInvalid = "Invalid OTP code.";
        public const string OtpMaxAttemptsReached = "Maximum verification attempts reached. Please request a new OTP.";
        public const string FailedToVerifyOtp = "Failed to verify OTP. Please try again.";

        // OTP Resend
        public const string OtpResentSuccessfully = "New OTP has been sent to your email.";
        public const string FailedToResendOtp = "Failed to resend OTP. Please try again.";
        public const string PendingUserNotFound = "No pending registration found for this email.";
        public const string RegistrationExpired = "Registration has expired. Please register again.";

        // User Activation
        public const string UserActivatedSuccessfully = "Your account has been activated successfully.";
        public const string FailedToActivateUser = "Failed to activate user account.";
        public const string PendingUserDataNotFound = "Pending registration data not found.";
    }
}
