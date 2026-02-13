using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.DataTransferObjects.SeatDTO;

namespace ServiceAbstraction
{
    public interface ISeatService
    {
        Task<PaginatedStruct<SeatInformationDTO>> GetAllSeatsAsync(SeatQueryStruct seatQuery);

        Task<SeatInformationDTO> GetSeatByIdAsync(int id);

        Task<bool> CreateSeatRowAsync(CreateSeatRowRequestDTO request);

        Task<bool> DeleteSeatAsync(DeleteSeatRequestDTO request);
    }
}
