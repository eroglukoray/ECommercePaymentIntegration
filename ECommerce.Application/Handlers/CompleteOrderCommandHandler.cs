using ECommerce.Application.Commands;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Handlers
{
    public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, bool>
    {
        private readonly IBalanceManagementService _balanceService;
        private readonly IOrderRepository _orderRepo;

        public CompleteOrderCommandHandler(
            IBalanceManagementService balanceService,
            IOrderRepository orderRepo)
        {
            _balanceService = balanceService;
            _orderRepo = orderRepo;
        }

        public async Task<bool> Handle(CompleteOrderCommand cmd, CancellationToken ct)
        {
            // 1) Siparişi DB’den al
            var order = await _orderRepo.GetByIdAsync(cmd.OrderId)
                        ?? throw new KeyNotFoundException($"Order {cmd.OrderId} bulunamadı.");

            // 2) Ödeme tamamlama çağrısını reservationId ile yap
            bool success = await _balanceService.CompleteOrderAsync(order.ReservationId);
            if (!success)
                throw new ApplicationException("Ödeme tamamlama başarısız.");

            // 3) Durumu güncelle ve kaydet
            order.Status = "Completed";
            await _orderRepo.UpdateAsync(order);

            return true;
        }

    }
}
