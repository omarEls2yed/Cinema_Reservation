using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DomainLayer.Contracts;
using DomainLayer.Models;
using Service.Specifications;
using ServiceAbstraction;
using Shared;
using Shared.DataTransferObjects.SeatDTO;
using StackExchange.Redis;
namespace Service
{
    public class SeatService(IUnitOfWorkRepository _unitOfWorkRepository,IMapper _mapper, IConnectionMultiplexer _muxer) : ISeatService
    {
        private readonly IDatabase _redis = _muxer.GetDatabase();
        public async Task<PaginatedStruct<SeatInformationDTO>> GetAllSeatsAsync(SeatQueryStruct seatQuery)
        {
            var specification1 = new SeatTicketSpecification(seatQuery);
            var seatRepository = _unitOfWorkRepository.GetRepository<Seat>();
            var seatsAfterSpecification1 = await seatRepository.GetAllAsync(specification1);
            var seatsInformationDTOMapper = _mapper.Map<IEnumerable<SeatInformationDTO>>(seatsAfterSpecification1).ToList();
            int totalSeatsInDb = await seatRepository.CountAsync(specification1);
            await UpdateSeatStatusesFromRedisAsync(seatQuery.EventId, seatsInformationDTOMapper);
            return new PaginatedStruct<SeatInformationDTO>(seatQuery.DesiredPageIndx, seatQuery.EachPageSize, totalSeatsInDb, seatsInformationDTOMapper);
        }

        private async Task UpdateSeatStatusesFromRedisAsync(int eventId, List<SeatInformationDTO> seats)
        {
            if (!seats.Any()) return;
            var keys = seats.Select(s => (RedisKey)$"lock:session:{eventId}:seat:{s.Id}").ToArray();
            var values = await _redis.StringGetAsync(keys);
            for (int i = 0; i < seats.Count; i++)
            {
                if (seats[i].Status == SeatStatusOptions.available && values[i].HasValue)
                    seats[i].Status = SeatStatusOptions.locked;
            }
        }
        public async Task<SeatInformationDTO> GetSeatByIdAsync(int id)
        {
            var seatRepository = _unitOfWorkRepository.GetRepository<Seat>();
            var seat = await seatRepository.GetByIdAsync(id);
            var seatsInformationDTOMapper = _mapper.Map<SeatInformationDTO>(seat);
            return seatsInformationDTOMapper;
        }

        public async Task<bool> CreateSeatRowAsync(CreateSeatRowRequestDTO request)
        {
            var seatRepository = _unitOfWorkRepository.GetRepository<Seat>();
            var newSeats = new List<Seat>();
            for (int i = 1; i <= request.SeatCount; i++)
            {
                var seat = new Seat
                {
                    VenueId = request.VenueId,
                    Row = request.Row,      
                    Number = i,                   
                    Class = request.Class        
                }; newSeats.Add(seat);
            }
            await seatRepository.AddRangeAsync(newSeats);
            var saved = await _unitOfWorkRepository.CompleteAsync();
            return saved > 0;
        }

        public async Task<bool> DeleteSeatAsync(DeleteSeatRequestDTO request)
        {
            var seatRepository = _unitOfWorkRepository.GetRepository<Seat>();
            var spec = new SeatByPositionSpecification(request.VenueId, request.Row, request.Number);
            var seat = await seatRepository.GetByIdAsync(spec);
            if (seat == null) return false;
            seatRepository.Delete(seat);
            var saved = await _unitOfWorkRepository.CompleteAsync();
            return saved > 0;
        }
    }
}
