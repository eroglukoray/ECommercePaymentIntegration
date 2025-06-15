using ECommerce.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Validators
{
    public class OrderResultDtoValidator : AbstractValidator<OrderResultDto>
    {
        public OrderResultDtoValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("OrderId boş olamaz.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status boş olamaz.")
                .Must(s => s == "Reserved" || s == "Completed")
                .WithMessage("Status 'Reserved' veya 'Completed' olmalıdır.");
        }
    }
}
