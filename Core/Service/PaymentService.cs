using DomainLayer.Contracts;
using DomainLayer.Models;
using Microsoft.Extensions.Logging;
using Service.Specifications;
using ServiceAbstraction;
using Shared.DataTransferObjects.PaymentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class PaymentService(IUnitOfWorkRepository unitOfWork, ILogger<PaymentService> logger) : IPaymentService
    {
        public async Task<PaymentResponseDTO> ProcessPaymentAsync(PaymentRequestDTO request)
        {
            logger.LogInformation($"Initiating Payment for User {request.UserId}, Amount: ${request.Amount}");
            try
            {
                if (request.Amount <= 0)
                    return new PaymentResponseDTO { IsSuccess = false, Message = "Invalid Amount" };

                var spec = new UserCardsSpecification(request.UserId);

                var cardRepo = unitOfWork.GetRepository<UserPaymentMethod>();
                var allUserCards = await cardRepo.GetAllAsync(spec);

                var defaultCard = allUserCards.FirstOrDefault(c => c.IsDefault);

                if (defaultCard == null)
                    return new PaymentResponseDTO { IsSuccess = false, Message = "No payment method found. Please add a card." };

                logger.LogInformation($"Sending Token {defaultCard.ProviderToken} to Provider...");
                await Task.Delay(1000);
                
                if (request.Amount > 10000)
                {
                    logger.LogWarning("Payment Declined: Insufficient Funds");
                    return new PaymentResponseDTO { IsSuccess = false, Message = "Declined: Insufficient Funds" };
                }

                if (defaultCard.ProviderToken == "pm_blocked_card")
                    return new PaymentResponseDTO { IsSuccess = false, Message = "Declined: Card blocked by issuer." };

                var transactionId = "txn_" + Guid.NewGuid().ToString("N").Substring(0, 10);
                logger.LogInformation($"Payment Successful. Txn ID: {transactionId}");

                return new PaymentResponseDTO
                {
                    IsSuccess = true,
                    TransactionId = transactionId,
                    Message = "Payment successful."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Stripe Payment System Error");
                return new PaymentResponseDTO { IsSuccess = false, Message = "Payment Provider Error. Please try again." };
            }
        }

        public async Task AddNewCardAsync(int userId, string token, string last4)
        {
            var repo = unitOfWork.GetRepository<UserPaymentMethod>();

            var spec = new UserCardsSpecification(userId);
            var existingCards = await repo.GetAllAsync(spec);
            foreach (var c in existingCards)
            {
                c.IsDefault = false;
            }
            var newCard = new UserPaymentMethod
            {
                UserId = userId,
                ProviderToken = token,
                Last4Digits = last4,
                CardBrand = "Visa",
                IsDefault = true
            };

            await repo.AddAsync(newCard);
            await unitOfWork.CompleteAsync();
        }

        public async Task<bool> RefundPaymentAsync(string transactionId)
        {
            try
            {
                if (string.IsNullOrEmpty(transactionId)) return false;
                logger.LogInformation($"Attempting Refund for Txn: {transactionId}");
                await Task.Delay(1500);
                if (transactionId.StartsWith("txn_fail")) return false;
                logger.LogInformation("Refund Successful.");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Refund System Error");
                return false;
            }
        }
    }
}

