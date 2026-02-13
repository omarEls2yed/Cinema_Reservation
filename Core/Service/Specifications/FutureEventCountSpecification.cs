using DomainLayer.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Specifications
{
    public class FutureEventCountSpecification : BaseSpecifications<Event>
    {
        public FutureEventCountSpecification(EventQueryStruct query)
            : base(e =>
                e.EventDate > DateTime.Now &&
                (string.IsNullOrEmpty(query.SearchTerm) || e.Name.Contains(query.SearchTerm))
            )
        {
        }
    }
}
