using AutoMapper;
using DomainLayer.Contracts;
using DomainLayer.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Service.Specifications;
using ServiceAbstraction;
using Shared;
using Shared.DataTransferObjects.EventDTO;
using StackExchange.Redis;

namespace Service
{
    public class EventService(IUnitOfWorkRepository unitOfWork,IConnectionMultiplexer _muexer,IMapper _mapper) : IEventService
    {
        private readonly IDatabase _redis = _muexer.GetDatabase();
        public async Task<PaginatedStruct<EventInformationDTO>> GetFutureEventsAsync(EventQueryStruct query)
        {
            string cacheKey = $"events_future_p{query.DesiredPageIndx}_s_{query.SearchTerm}";
            string? cachedData = await _redis.StringGetAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
                return JsonConvert.DeserializeObject<PaginatedStruct<EventInformationDTO>>(cachedData)!;
            var countSpec = new FutureEventCountSpecification(query);
            var totalCount = await unitOfWork.GetRepository<Event>().CountAsync(countSpec);
            var spec = new FutureEventSpecification(query);
            var events = await unitOfWork.GetRepository<Event>().GetAllAsync(spec);
            var eventDtos = _mapper.Map<IEnumerable<EventInformationDTO>>(events);
            var result = new PaginatedStruct<EventInformationDTO>(query.DesiredPageIndx, query.EachPageSize, totalCount, eventDtos);
            await _redis.StringSetAsync(cacheKey, JsonConvert.SerializeObject(result), TimeSpan.FromMinutes(10));
            return result;
        }

        public async Task<bool> AddFutureEventAsync(CreateEventRequestDTO request)
        {
            var repo = unitOfWork.GetRepository<Event>();

            var checkSpec = new EventDuplicateSpecification(request.Name, request.VenueId, request.EventDate);
            var existingEvent = await repo.GetByIdAsync(checkSpec);

            if (existingEvent != null)
            {
                return false;
            }

            try
            {
                var newEvent = new Event
                {
                    Name = request.Name,
                    EventDate = request.EventDate,
                    VenueId = request.VenueId,
                    IsActive = true,
                    BasePrice = request.BasePrice
                };

                await repo.AddAsync(newEvent);
                await unitOfWork.CompleteAsync();

                await DeleteKeysByPatternAsync("events_future_*");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            try
            {
                var repo = unitOfWork.GetRepository<Event>();
                var eventEntity = await repo.GetByIdAsync(id);
                if (eventEntity == null)
                    return false;
                repo.Delete(eventEntity);
                await unitOfWork.CompleteAsync();
                await _redis.KeyDeleteAsync($"event_details_{id}");
                await DeleteKeysByPatternAsync("events_future_*");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateEvent(UpdateEventRequestDTO query)
        {
            try
            {
                var repo = unitOfWork.GetRepository<Event>();
                var eventEntity = await repo.GetByIdAsync(query.Id);
                if (eventEntity == null) return false;
                eventEntity.EventDate = query.NewEventDate;
                eventEntity.Name = query.NewName;
                eventEntity.EventDate = query.NewEventDate;
                eventEntity.BasePrice = query.BasePrice;
                repo.Update(eventEntity);
                await unitOfWork.CompleteAsync();
                await _redis.KeyDeleteAsync($"event_details_{query.Id}");
                await DeleteKeysByPatternAsync("events_future_*");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private async Task DeleteKeysByPatternAsync(string pattern)
        {
            var endpoints = _muexer.GetEndPoints();
            var server = _muexer.GetServer(endpoints.First());

            if (!server.IsConnected) return;

            var keys = server.KeysAsync(pattern: pattern);

            await foreach (var key in keys)
            {
                await _redis.KeyDeleteAsync(key);
            }
        }
    }
}
