using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;


namespace ECommerce.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ProductDto ↔ Domain.Product
            CreateMap<ProductDto, Product>();
            CreateMap<Product, ProductDto>();

            // Order ↔ OrderResultDto
            CreateMap<Order, OrderResultDto>()
                .ForMember(dst => dst.OrderId, opt => opt.MapFrom(src => src.Id));

            // CreateOrderRequest içindeki ProductIds zaten kullanılıyor.
        }
    }
}
