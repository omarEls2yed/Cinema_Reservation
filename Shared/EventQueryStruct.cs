using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class EventQueryStruct
    {
        public string? SearchTerm { get; set; }
        public int DesiredPageIndx { get; set; } = 1;

        private int eachPageSize = 5;

        public int EachPageSize
        {
            get => eachPageSize;
            set => eachPageSize = value > 10 ? 10 : value;
        }
    }
}
