using DomainLayer.Models;
using Shared;
using Shared.DataTransferObjects.EventDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace Service.Specifications
{
    public class FutureEventSpecification : BaseSpecifications<Event>
    {
        public FutureEventSpecification(EventQueryStruct query) : base(e =>
            e.EventDate > DateTime.Now &&
            (string.IsNullOrEmpty(query.SearchTerm) || e.Name.Contains(query.SearchTerm))
        )
        {
            AddIncludeExpression(e => e.Venue);
            AddOrderBy(e => e.EventDate);
            ApplyPagination(query.EachPageSize, query.DesiredPageIndx);
        }
    }
}
