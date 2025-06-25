namespace ASI.Basecode.Services.ServiceModels
{
    public class PasswordResetViewModel
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
} 