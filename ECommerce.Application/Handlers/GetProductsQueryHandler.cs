using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries;
using MediatR;

namespace ECommerce.Application.Handlers
{

    public class GetProductsQueryHandler
         : IRequestHandler<GetProductsQuery, List<ProductDto>>
    {
        private readonly IBalanceManagementService _balanceSvc;
        private readonly IMapper _mapper;
        public GetProductsQueryHandler(
            IBalanceManagementService svc, IMapper mapper)
        {
            _balanceSvc = svc; _mapper = mapper;
        }

        public async Task<List<ProductDto>> Handle(GetProductsQuery q, CancellationToken ct)
        {
            var products = await _balanceSvc.GetProductsAsync();
            return _mapper.Map<List<ProductDto>>(products);
        }
    }
}
