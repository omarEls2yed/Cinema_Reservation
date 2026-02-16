using DomainLayer.Contracts;
using DomainLayer.Models;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Specifications;
using ServiceAbstraction;
using Shared;
using Shared.DataTransferObjects.PaymentDTO;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Consumers
{
    public class BookingConsumer(
        IServiceProvider _serviceProvider,
        ILogger<BookingConsumer> _logger)
        : IConsumer<BookingMessage>
    {
        public async Task Consume(ConsumeContext<BookingMessage> context)
        {
            var msg = context.Message;
            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWorkRepository>();
            var paymentSvc = scope.ServiceProvider.GetRequiredService<IPaymentService>();
            var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase();

            string idempotencyKey = $"processing_booking:{msg.BookingId}";
            bool isNewRequest = await redis.StringSetAsync(idempotencyKey, "locked", TimeSpan.FromMinutes(10), When.NotExists);
            if (!isNewRequest) return;

            try
            {
                var ticketRepository = uow.GetRepository<Ticket>();
                var spec = new TicketExistenceSpecification(msg.EventId, msg.SeatId);

                if (await ticketRepository.CountAsync(spec) > 0)
                {
                    await PublishFailure(context, msg, "Seat already sold.");
                    return;
                }

                var paymentResult = await paymentSvc.ProcessPaymentAsync(new PaymentRequestDTO
                {
                    UserId = msg.UserId,
                    Amount = msg.TicketPrice
                });

                if (!paymentResult.IsSuccess)
                {
                    await PublishFailure(context, msg, paymentResult.Message);
                    return;
                }

                try
                {
                    var ticket = new Ticket
                    {
                        UserId = msg.UserId,
                        EventId = msg.EventId,
                        SeatId = msg.SeatId,
                        Price = msg.TicketPrice,
                        TicketCode = $"TKT-{msg.BookingId:N}".Substring(0, 12).ToUpper()
                    };

                    await ticketRepository.AddAsync(ticket);
                    await uow.CompleteAsync();

                    await context.Publish(new BookingCompletedEvent
                    {
                        BookingId = msg.BookingId,
                        UserId = msg.UserId,
                        IsSuccess = true,
                        TicketCode = ticket.TicketCode
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DB Save Failed. Refunding {BookingId}", msg.BookingId);
                    await paymentSvc.RefundPaymentAsync(paymentResult.TransactionId);
                    await PublishFailure(context, msg, "Database error. Payment refunded.");
                }
            }
            finally
            {
                string lockKey = $"lock:event:{msg.EventId}:seat:{msg.SeatId}";
                string processingKey = $"processing:event:{msg.EventId}:seat:{msg.SeatId}";

                await redis.KeyDeleteAsync(lockKey);
                await redis.KeyDeleteAsync(processingKey);
            }
        }

        private static async Task PublishFailure(ConsumeContext<BookingMessage> context, BookingMessage msg, string errorMessage)
        {
            await context.Publish(new BookingCompletedEvent
            {
                BookingId = msg.BookingId,
                UserId = msg.UserId,
                IsSuccess = false,
                ErrorMessage = errorMessage
            });
        }
    }
}