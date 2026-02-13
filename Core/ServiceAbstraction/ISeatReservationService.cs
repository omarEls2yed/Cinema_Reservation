using Shared.DataTransferObjects.SeatDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction
{
    public interface ISeatReservationService
    {
        Task<bool> LockSeatAsync(LockSeatRequestDTO request);
        
        Task<bool> UnlockSeatAsync(UnlockSeatRequestDTO request);

        Task<string> BookSeatAsync(BookTicketRequestDTO request);
    }
}
