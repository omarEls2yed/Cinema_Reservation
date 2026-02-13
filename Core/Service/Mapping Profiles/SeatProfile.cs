using AutoMapper;
using Shared.DataTransferObjects.SeatDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DomainLayer.Models;
using Shared;

namespace Service.Mapping_Profiles
{
    public class SeatProfile : Profile
    {
        public SeatProfile()
        {
            CreateMap<Seat, SeatInformationDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.Tickets.Any() ? SeatStatusOptions.booked : SeatStatusOptions.available)).ReverseMap();
        }
    }
}
