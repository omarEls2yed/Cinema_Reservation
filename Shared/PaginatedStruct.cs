using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PaginatedStruct<T>
    {
        public PaginatedStruct(int desiredPageIndx, int eachPageSize, int totalCount, IEnumerable<T> data)
        {
            DesiredPageIndx = desiredPageIndx;
            EachPageSize = eachPageSize;
            TotalCount = totalCount;
            Data = data;
        }
        public int DesiredPageIndx { get; set; }
        public int EachPageSize { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
}
