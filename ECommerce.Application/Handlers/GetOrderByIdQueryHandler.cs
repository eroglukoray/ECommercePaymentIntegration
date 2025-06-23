using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries;
using MediatR;

namespace ECommerce.Application.Handlers
{
    public class GetOrderByIdQueryHandler
          : IRequestHandler<GetOrderByIdQuery, OrderResultDto?>
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(
            IOrderRepository orderRepo,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
        }

        public async Task<OrderResultDto?> Handle(
            GetOrderByIdQuery request,
            CancellationToken cancellationToken)
        {
            // 1) Veritabanından Order entity’sini çek
            var order = await _orderRepo.GetByIdAsync(request.OrderId);
            if (order == null)
                return null;

            // 2) Domain Order → DTO map
            var dto = _mapper.Map<OrderResultDto>(order);
            return dto;
        }
    }
}
