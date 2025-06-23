using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs
{
    /// <summary>
    /// Ödeme tamamlama sonucunda dönen zarflanmış yanıt.
    /// </summary>
    public class CompleteResponseEnvelope
    {
        /// <summary>
        /// İşlem yapılan siparişin kimliği.
        /// </summary>
        public string OrderId { get; set; } = default!;

        /// <summary>
        /// Ödemenin başarılı şekilde tamamlanıp tamamlanmadığı.
        /// </summary>
        public bool Success { get; set; }
    }
}
