namespace MealPrep.BLL.Services.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email với nội dung text thông thường
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body);

        /// <summary>
        /// Gửi email với nội dung HTML
        /// </summary>
        Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody);

        /// <summary>
        /// Gửi OTP code qua email
        /// </summary>
        Task SendOtpEmailAsync(string toEmail, string otpCode);
    }
}
