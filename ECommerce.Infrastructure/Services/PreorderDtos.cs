using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Services
{
    // 1) Request gövdesi artık tek bir orderId ve amount içeriyor
    public class PreorderRequest
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }

    // 2) Response Envelope, eskisiyle aynı kalıyor
    public class PreorderResponseEnvelope
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public PreorderResponseData Data { get; set; } = new();
    }

    public class PreorderResponseData
    {
        [JsonPropertyName("preOrder")]
        public PreOrderInfo PreOrder { get; set; } = new();

        [JsonPropertyName("updatedBalance")]
        public UpdatedBalanceInfo UpdatedBalance { get; set; } = new();
    }

    public class PreOrderInfo
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("completedat")]
        public DateTime CompletedAt { get; set; }
        [JsonPropertyName("cancelledat")]
        public DateTime CancelledAt { get; set; }
    }

    public class UpdatedBalanceInfo
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("totalBalance")]
        public long TotalBalance { get; set; }

        [JsonPropertyName("availableBalance")]
        public long AvailableBalance { get; set; }

        [JsonPropertyName("blockedBalance")]
        public long BlockedBalance { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }
    }
}
