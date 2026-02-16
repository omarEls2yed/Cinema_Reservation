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
using DomainLayer.Exceptions;
using MassTransit;
using Shared;
using Event = DomainLayer.Models.Event;

namespace Service
{
    public class SeatReservationService(
        IUnitOfWorkRepository _unitOfWorkRepository,
        IConnectionMultiplexer _muexer,
        IPublishEndpoint _publishEndpoin) : ISeatReservationService
    {
        private readonly IDatabase _redis = _muexer.GetDatabase();
        private string GetLockKey(int eventId, int seatId) => $"lock:event:{eventId}:seat:{seatId}";
        public async Task<bool> LockSeatAsync(LockSeatRequestDTO request)
        {
            var targetSeat = await _unitOfWorkRepository.GetRepository<Seat>().GetByIdAsync(request.SeatId);
            var targetEvent = await _unitOfWorkRepository.GetRepository<Event>().GetByIdAsync(request.EventId);

            if (targetSeat == null) throw new SeatNotFoundException(request.SeatId);
            if (targetEvent == null) throw new EventNotFoundException(request.EventId);

            string processingKey = $"processing:event:{request.EventId}:seat:{request.SeatId}";
            if (await _redis.KeyExistsAsync(processingKey)) return false;

            var ticketRepository = _unitOfWorkRepository.GetRepository<Ticket>();
            var spec = new TicketExistenceSpecification(request.EventId, request.SeatId);
            if (await ticketRepository.CountAsync(spec) > 0) return false;

            string key = GetLockKey(request.EventId, request.SeatId);
            string value = request.UserId.ToString();
            return await _redis.StringSetAsync(key, value, TimeSpan.FromMinutes(10), When.NotExists);
        }

        public async Task<bool> UnlockSeatAsync(UnlockSeatRequestDTO request)
        {
            var targetSeat = await _unitOfWorkRepository.GetRepository<Seat>().GetByIdAsync(request.SeatId);
            var targetEvent = await _unitOfWorkRepository.GetRepository<Event>().GetByIdAsync(request.EventId);

            if (targetSeat == null) throw new SeatNotFoundException(request.SeatId);
            if (targetEvent == null) throw new EventNotFoundException(request.EventId);

            string processingKey = $"processing:event:{request.EventId}:seat:{request.SeatId}";
            if (await _redis.KeyExistsAsync(processingKey)) return false;

            string key = GetLockKey(request.EventId, request.SeatId);
            var currentLockValue = await _redis.StringGetAsync(key);

            if (!currentLockValue.HasValue || currentLockValue.ToString() != request.UserId.ToString())
                return false;

            return await _redis.KeyDeleteAsync(key);
        }

        public async Task<string> BookSeatAsync(BookTicketRequestDTO request)
        {
            var targetSeat = await _unitOfWorkRepository.GetRepository<Seat>().GetByIdAsync(request.SeatId);
            var targetEvent = await _unitOfWorkRepository.GetRepository<Event>().GetByIdAsync(request.EventId);

            if (targetSeat == null) throw new SeatNotFoundException(request.SeatId);
            if (targetEvent == null) throw new EventNotFoundException(request.EventId);

            string lockKey = GetLockKey(request.EventId, request.SeatId);
            string processingKey = $"processing:event:{request.EventId}:seat:{request.SeatId}";
            string userIdStr = request.UserId.ToString();

            bool acquiredProcessing = await _redis.StringSetAsync(processingKey, userIdStr, TimeSpan.FromMinutes(2), When.NotExists);
            if (!acquiredProcessing)
            {
                return "Booking Failed: A request for this seat is already being processed. Please wait.";
            }

            var currentLockValue = await _redis.StringGetAsync(lockKey);
            if (currentLockValue.HasValue && currentLockValue.ToString() != userIdStr)
            {
                await _redis.KeyDeleteAsync(processingKey);
                return "Booking Failed: Seat is currently locked by another user.";
            }

            var ticketRepo = _unitOfWorkRepository.GetRepository<Ticket>();
            var spec = new TicketExistenceSpecification(request.EventId, request.SeatId);
            if (await ticketRepo.CountAsync(spec) > 0)
            {
                await _redis.KeyDeleteAsync(processingKey);
                return "Booking Failed: Seat already sold.";
            }

            var trackingId = Guid.NewGuid();
            var price = targetSeat.Class.ToLower() == "vip" ? targetEvent.BasePrice * 2 : targetEvent.BasePrice;

            var message = new BookingMessage()
            {
                UserId = request.UserId,
                EventId = request.EventId,
                SeatId = request.SeatId,
                TicketPrice = price,
                BookingId = trackingId,
                RequestTime = DateTime.UtcNow
            };

            await _publishEndpoin.Publish(message);
            return $"Processing: Your tracking ID is {trackingId}. We are verifying your seat.";
        }
    }
}

