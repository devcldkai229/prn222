using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BusinessObjects.Entities;
using MealPrep.BLL.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MealPrep.BLL.Services;

public class MomoPaymentService : IMomoService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MomoPaymentService> _logger;

    public MomoPaymentService(
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<MomoPaymentService> logger
    )
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> CreatePaymentRequestAsync(
        Payment payment,
        string returnUrl,
        string ipnUrl
    )
    {
        var partnerCode =
            _configuration["Momo:PartnerCode"]
            ?? throw new InvalidOperationException("Momo:PartnerCode is not configured");
        var accessKey =
            _configuration["Momo:AccessKey"]
            ?? throw new InvalidOperationException("Momo:AccessKey is not configured");
        var secretKey =
            _configuration["Momo:SecretKey"]
            ?? throw new InvalidOperationException("Momo:SecretKey is not configured");
        var apiUrl =
            _configuration["Momo:ApiUrl"]
            ?? throw new InvalidOperationException("Momo:ApiUrl is not configured");

        // Generate requestId and orderId
        var requestId = Guid.NewGuid().ToString();
        var orderId = payment.PaymentCode;

        // Prepare request data
        var requestData = new
        {
            partnerCode = partnerCode,
            requestId = requestId,
            amount = (long)payment.Amount,
            orderId = orderId,
            orderInfo = payment.Description ?? $"Thanh toan subscription #{payment.SubscriptionId}",
            redirectUrl = returnUrl,
            ipnUrl = ipnUrl,
            requestType = "captureWallet",
            extraData = "",
            lang = "vi",
        };

        // Generate signature
        var rawSignature =
            $"accessKey={accessKey}&amount={requestData.amount}&extraData={requestData.extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={requestData.orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestData.requestType}";
        var signature = ComputeHmacSha256(rawSignature, secretKey);

        // Add signature to request
        var requestBody = new
        {
            requestData.partnerCode,
            requestData.requestId,
            requestData.amount,
            requestData.orderId,
            requestData.orderInfo,
            requestData.redirectUrl,
            requestData.ipnUrl,
            requestData.requestType,
            requestData.extraData,
            requestData.lang,
            signature = signature,
        };

        // Serialize to JSON (MoMo expects exact property names)
        var jsonContent = JsonSerializer.Serialize(requestBody);

        _logger.LogInformation("Sending MoMo payment request: {Request}", jsonContent);

        // Send POST request
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(apiUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("MoMo payment response: {Response}", responseContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"MoMo API error: {response.StatusCode} - {responseContent}"
            );
        }

        // Parse response
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        if (
            responseData.TryGetProperty("resultCode", out var resultCode)
            && resultCode.GetInt32() == 0
            && responseData.TryGetProperty("payUrl", out var payUrl)
        )
        {
            return payUrl.GetString() ?? throw new InvalidOperationException("MoMo payUrl is null");
        }

        var message = responseData.TryGetProperty("message", out var msg)
            ? msg.GetString()
            : "Unknown error";
        throw new InvalidOperationException($"MoMo payment failed: {message}");
    }

    private string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public bool VerifySignature(string rawData, string signature)
    {
        try
        {
            var secretKey =
                _configuration["Momo:SecretKey"]
                ?? throw new InvalidOperationException("Momo:SecretKey is not configured");

            var computedSignature = ComputeHmacSha256(rawData, secretKey);
            return computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify signature");
            return false;
        }
    }
}
