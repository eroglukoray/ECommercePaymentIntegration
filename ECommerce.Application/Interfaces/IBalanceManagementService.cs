using ECommerce.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IBalanceManagementService
    {
        Task<List<ProductDto>> GetProductsAsync();
        Task<string> PreorderAsync(string orderId, int amount);
        Task<bool> CompleteOrderAsync(string reservationId);
    }
}
