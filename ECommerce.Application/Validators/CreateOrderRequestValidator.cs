using ECommerce.Application.DTOs;
using FluentValidation;


namespace ECommerce.Application.Validators
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.ProductIds)
                .NotNull().WithMessage("ProductIds boş olamaz.")
                .NotEmpty().WithMessage("En az bir ürün seçmelisiniz.");
        }
    }
}
