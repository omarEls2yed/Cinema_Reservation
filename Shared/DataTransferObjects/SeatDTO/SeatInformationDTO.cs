using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.SeatDTO
{
    public class SeatInformationDTO
    {
        public int Id { get; set; }

        public int Row { get; set; }

        public int Number { get; set; }

        public string Class { get; set; }

        public int VenueId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SeatStatusOptions Status { get; set; }
    }
}
