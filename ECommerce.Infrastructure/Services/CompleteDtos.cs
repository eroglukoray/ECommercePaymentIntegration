using System.Text.Json.Serialization;

namespace ECommerce.Infrastructure.Services
{
    public class CompleteResponseEnvelope
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public CompleteResponseData Data { get; set; } = new();
    }

    public class CompleteResponseData
    {
        [JsonPropertyName("order")]
        public CompletedOrderInfo Order { get; set; } = new();

        [JsonPropertyName("updatedBalance")]
        public UpdatedBalanceInfo UpdatedBalance { get; set; } = new();
    }

    public class CompletedOrderInfo
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("completedAt")]
        public DateTime CompletedAt { get; set; }
    }
}
