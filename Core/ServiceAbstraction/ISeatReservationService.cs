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
        Task<bool> LockSeatAsync(LockSeatRequestDTO request, string userId);
        
        Task<bool> UnlockSeatAsync(UnlockSeatRequestDTO request, string userId);

        Task<string> BookSeatAsync(BookTicketRequestDTO request, string userId);
    }
}
