using Shared.DataTransferObjects.PaymentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> ProcessPaymentAsync(PaymentRequestDTO request);
        Task<bool> RefundPaymentAsync(string transactionId);
        Task AddNewCardAsync(int userId, string fakeToken, string last4);
    }
}
