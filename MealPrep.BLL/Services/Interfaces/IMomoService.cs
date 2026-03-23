using BusinessObjects.Entities;

namespace MealPrep.BLL.Services.Interfaces;

public interface IMomoService
{
    Task<string> CreatePaymentRequestAsync(Payment payment, string returnUrl, string ipnUrl);
    
    /// <summary>
    /// Verify signature from MoMo callback
    /// </summary>
    bool VerifySignature(string rawData, string signature);
}
