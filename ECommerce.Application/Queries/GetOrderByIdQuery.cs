using ECommerce.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Queries
{
    /// <summary>
    /// Belirli bir siparişi ID’si ile sorgulamak için kullanılır.
    /// </summary>
    public record GetOrderByIdQuery(string OrderId) : IRequest<OrderResultDto?>;
}
