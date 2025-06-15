using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Services
{
    public class CompleteRequest
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;
    }
}
