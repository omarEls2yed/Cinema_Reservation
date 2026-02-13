using Shared;
using Shared.DataTransferObjects.EventDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction
{
    public interface IEventService
    {
        Task<PaginatedStruct<EventInformationDTO>> GetFutureEventsAsync(EventQueryStruct query);
        Task<bool> AddFutureEventAsync(CreateEventRequestDTO request);
        Task<bool> DeleteEventAsync(int id);
        Task<bool> UpdateEvent(UpdateEventRequestDTO request);
    }
}
