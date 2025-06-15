using ECommerce.Application.Commands;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Handlers
{
    public class CreateOrderCommandHandler
      : IRequestHandler<CreateOrderCommand, OrderResultDto>
    {
        private readonly IBalanceManagementService _balanceService;
        private readonly IOrderRepository _orderRepo;

        public CreateOrderCommandHandler(
            IBalanceManagementService balanceService,
            IOrderRepository orderRepo)
        {
            _balanceService = balanceService;
            _orderRepo = orderRepo;
        }

        public async Task<OrderResultDto> Handle(
            CreateOrderCommand cmd,
            CancellationToken ct)
        {
            // 1) Dış servisten ürünleri al
            var all = await _balanceService.GetProductsAsync();

            // 2) Gelen ID listesiyle filtrele
            var selectedDtos = all
                .Where(p => cmd.Request.ProductIds.Contains(p.Id))
                .ToList();

            if (!selectedDtos.Any())
                throw new ApplicationException("Geçerli ürün bulunamadı.");

            // 3) DTO → Entity map
            var products = selectedDtos
                .Select(dto => new Product
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Currency = dto.Currency,
                    Category = dto.Category,
                    Stock = dto.Stock
                })
                .ToList();

            // 4) Order yarat (henüz Pending, ReservationId boş)
            var order = new Order
            {
                Products = products,
                Status = "Pending",
                ReservationId = string.Empty
            };

            // 5) DB’ye ekle ki order.Id oluşsun
            await _orderRepo.AddAsync(order);

            // 6) Pre-order için total tutarı hesapla
            int totalAmount = (int)order.TotalPrice;

            // 7) Dış servise pre-order isteği
            var reservationId = await _balanceService
                .PreorderAsync(order.Id, totalAmount);

            if (string.IsNullOrEmpty(reservationId))
                throw new ApplicationException("Pre-order başarısız.");

            // 8) **Çok Önemli:** Status ve ReservationId değerlerini set edin
            order.Status = "Reserved";
            order.ReservationId = reservationId;

            // 9) Güncellenmiş order’ı DB’de Update et
            await _orderRepo.UpdateAsync(order);

            // 10) Sonucu dön
            return new OrderResultDto
            {
                OrderId = order.Id,
                Status = order.Status
            };
        }
    }
}