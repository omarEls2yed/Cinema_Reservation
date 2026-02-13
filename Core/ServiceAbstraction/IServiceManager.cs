using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction
{
    public interface IServiceManager
    {
        ISeatReservationService SeatReservationService { get; }
        ISeatService SeatService { get; }
        IEventService EventService { get; }
        IPaymentService PaymentService { get; }
    }
}
