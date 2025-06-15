using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators
{
    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        public ProductDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Product Id boş olamaz.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product adı boş olamaz.")
                .MaximumLength(200).WithMessage("Product adı en fazla 200 karakter olabilir.");

            RuleFor(x => x.Price)
                .GreaterThan(0m).WithMessage("Product fiyatı sıfırdan büyük olmalıdır.");
        }
    }
}
