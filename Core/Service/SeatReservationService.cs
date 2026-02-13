using DomainLayer.Contracts;
using DomainLayer.Models;
using Microsoft.Extensions.Logging;
using Service.Specifications;
using ServiceAbstraction;
using Shared.DataTransferObjects.PaymentDTO;
using Shared.DataTransferObjects.SeatDTO;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class SeatReservationService(IUnitOfWorkRepository _unitOfWorkRepository,IConnectionMultiplexer _muexer, IPaymentService paymentService) : ISeatReservationService
    {
        private readonly IDatabase _redis = _muexer.GetDatabase();
        private string GetLockKey(int eventId, int seatId) => $"lock:event:{eventId}:seat:{seatId}";
        public async Task<bool> LockSeatAsync(LockSeatRequestDTO request)
        {
            var ticketRepository = _unitOfWorkRepository.GetRepository<Ticket>();
            var spec = new TicketExistenceSpecification(request.EventId, request.SeatId);
            var existingTicketCount = await ticketRepository.CountAsync(spec);
            if (existingTicketCount > 0) return false;
            string key = GetLockKey(request.EventId, request.SeatId);
            string value = request.UserId.ToString();
            return await _redis.StringSetAsync(key, value, TimeSpan.FromMinutes(10), When.NotExists);
        }

        public async Task<bool> UnlockSeatAsync(UnlockSeatRequestDTO request)
        {
            string key = GetLockKey(request.EventId, request.SeatId);
            var currentLockValue = await _redis.StringGetAsync(key);
            if (!currentLockValue.HasValue || currentLockValue.ToString() != request.UserId.ToString()) return false;
            return await _redis.KeyDeleteAsync(key);
        }

        public async Task<string> BookSeatAsync(BookTicketRequestDTO request)
        {
            string lockKey = GetLockKey(request.EventId, request.SeatId);
            string userIdStr = request.UserId.ToString();
            var currentLockValue = await _redis.StringGetAsync(lockKey);
            if (currentLockValue.HasValue)
            {
                if (currentLockValue.ToString() != userIdStr)
                    return "Booking Failed: Seat is currently locked by another user.";
            }
            else
            {
                bool acquired = await _redis.StringSetAsync(lockKey, userIdStr, TimeSpan.FromMinutes(2), When.NotExists);
                if (!acquired) return "Booking Failed: Seat was just taken.";
            }
            try
            {
                var ticketRepository = _unitOfWorkRepository.GetRepository<Ticket>();
                var spec = new TicketExistenceSpecification(request.EventId, request.SeatId);
                if (await ticketRepository.CountAsync(spec) > 0) return "Booking Failed: Seat already sold.";
                var paymentResult = await paymentService.ProcessPaymentAsync(new PaymentRequestDTO
                {
                    UserId = request.UserId,
                    Amount = request.TicketPrice
                });
                if (!paymentResult.IsSuccess) return $"Booking Failed: {paymentResult.Message}";
                try
                {
                    var ticket = new Ticket
                    {
                        UserId = request.UserId,
                        EventId = request.EventId,
                        SeatId = request.SeatId,
                        Price = request.TicketPrice,
                        TicketCode = $"TKT-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                    };
                    await ticketRepository.AddAsync(ticket);
                    await _unitOfWorkRepository.CompleteAsync();
                    return "Success";
                }
                catch (Exception)
                {
                    await paymentService.RefundPaymentAsync(paymentResult.TransactionId);
                    return "Error: Database failed. Payment refunded.";
                }
            }
            finally
            {
                await _redis.KeyDeleteAsync(lockKey);
            }
        }
    }
}

