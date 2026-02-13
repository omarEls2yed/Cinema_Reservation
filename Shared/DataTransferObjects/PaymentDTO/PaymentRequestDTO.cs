using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.PaymentDTO
{
    public class PaymentRequestDTO
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";

        public string PaymentMethodToken { get; set; }
    }
}
