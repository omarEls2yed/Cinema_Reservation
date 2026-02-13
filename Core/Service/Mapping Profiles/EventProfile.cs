using AutoMapper;
using DomainLayer.Models;
using Shared.DataTransferObjects.EventDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Mapping_Profiles
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventInformationDTO>()
                .ForMember(dest => dest.VenueName, opt =>
                    opt.MapFrom(src => src.Venue != null ? src.Venue.Name : "Unknown Venue"))
                .ForMember(dest => dest.VenueLocation, opt =>
                    opt.MapFrom(src => src.Venue != null ? src.Venue.Location : "Unknown Location"))
                .ForMember(dest => dest.IsActive, opt =>
                    opt.MapFrom(src => true)); 
        }
    }
}
