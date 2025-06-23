using ECommerce.Application.Commands;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Handlers
{
    /// <summary>
    /// CompleteOrderCommand için MediatR handler’ı.
    /// </summary>
    public class CompleteOrderCommandHandler
        : IRequestHandler<CompleteOrderCommand, CompleteResponseEnvelope>
    {
        private readonly IBalanceManagementService _balanceService;
        private readonly IOrderRepository _orderRepository;

        public CompleteOrderCommandHandler(
            IBalanceManagementService balanceService,
            IOrderRepository orderRepository)
        {
            _balanceService = balanceService;
            _orderRepository = orderRepository;
        }

        public async Task<CompleteResponseEnvelope> Handle(
            CompleteOrderCommand request,
            CancellationToken cancellationToken)
        {
            // 1) Siparişi DB'den al
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new ApplicationException(
                    $"Order with ID '{request.OrderId}' not found.");

            // 2) Balance Management servisinde tamamlama isteği
            var completed = await _balanceService.CompleteOrderAsync(request.OrderId);
            if (!completed)
                throw new ApplicationException("Ödeme tamamlama başarısız.");

            // 3) Sipariş durumunu güncelle
            order.Status = "Completed";
            await _orderRepository.UpdateAsync(order);

            // 4) Başarılı sonuç zarfı dön
            return new CompleteResponseEnvelope
            {
                OrderId = order.Id,
                Success = true
            };
        }
    }
}
