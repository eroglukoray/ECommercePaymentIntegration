using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries;
using MediatR;

namespace ECommerce.Application.Handlers
{

    public class GetProductsQueryHandler
         : IRequestHandler<GetProductsQuery, List<ProductDto>>
    {
        private readonly IBalanceManagementService _balanceService;

        public GetProductsQueryHandler(IBalanceManagementService balanceService)
        {
            _balanceService = balanceService;
        }

        public async Task<List<ProductDto>> Handle(
            GetProductsQuery request,
            CancellationToken cancellationToken)
        {
            // 1) Dış servisten 'success + data' envelope okundu,
            //    IBalanceManagementService zaten sadece List<ProductDto> döndürüyor.
            var products = await _balanceService.GetProductsAsync();

            // 2) Ek iş mantığı yoksa doğrudan dönüyoruz.
            return products;
        }
    }
}
