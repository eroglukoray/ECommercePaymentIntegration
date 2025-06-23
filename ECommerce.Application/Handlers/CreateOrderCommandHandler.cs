using AutoMapper;
using ECommerce.Application.Commands;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Repositories;
using MediatR;
using System.Text.Json;

namespace ECommerce.Application.Handlers
{
    public class CreateOrderCommandHandler
      : IRequestHandler<CreateOrderCommand, OrderResultDto>
    {
        private readonly IBalanceManagementService _balanceSvc;
        private readonly IOrderRepository _orderRepo;
        private readonly IIdempotencyRepository _idemRepo;
        private readonly IMapper _mapper;

    
        public CreateOrderCommandHandler(
            IBalanceManagementService balanceSvc,
            IOrderRepository orderRepo,
            IIdempotencyRepository idemRepo,
            IMapper mapper)
        {
            _balanceSvc = balanceSvc;
            _orderRepo = orderRepo;
            _idemRepo = idemRepo;
            _mapper = mapper;
        }

        public async Task<OrderResultDto> Handle(
            CreateOrderCommand cmd,
            CancellationToken ct)
        {
            // --- A) İdempotency Kontrolü ---
            var existing = await _idemRepo.GetByKeyAsync(cmd.IdempotencyKey);
            if (existing != null)
            {
                // Daha önce kaydettiğimiz JSON’u DTO’ya çevirip geri dönüyoruz
                return JsonSerializer
                    .Deserialize<OrderResultDto>(existing.ResultJson)!;
            }

            // --- B) Asıl Sipariş Oluşturma Akışı ---
            var products = await _balanceSvc.GetProductsAsync();
            var selected = products.Where(p => cmd.Request.ProductIds.Contains(p.Id)).ToList();
            if (!selected.Any())
                throw new ApplicationException("Geçerli ürün bulunamadı.");

            var order = new Order
            {
                Products = _mapper.Map<List<Product>>(selected),
                Status = "Pending"
            };
            await _orderRepo.AddAsync(order);

            var reservationId = await _balanceSvc.PreorderAsync(order.Id, (int)order.TotalPrice);
            if (string.IsNullOrEmpty(reservationId))
                throw new ApplicationException("Pre-order başarısız.");

            order.Status = "Reserved";
            order.ReservationId = reservationId;
            await _orderRepo.UpdateAsync(order);

            var resultDto = _mapper.Map<OrderResultDto>(order);

            // --- C) Idempotency Kayıt Etme ---
            var record = new IdempotencyRecord
            {
                Key = cmd.IdempotencyKey,
                ResultJson = JsonSerializer.Serialize(resultDto)
            };
            await _idemRepo.SaveAsync(record);

            return resultDto;
        }
    }
}