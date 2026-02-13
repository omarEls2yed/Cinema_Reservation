using DomainLayer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    static class SpecificationEvaluator
    {
        public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> spec) where T : class
        {
            var query = inputQuery;
            if (spec.WhereExpression is not null) query = query.Where(spec.WhereExpression);
            foreach (var func in spec.CustomQuerys) 
                query = func(query);
            
            if (spec.OrderBy is not null)
            {
                var orderedQuery = query.OrderBy(spec.OrderBy);
                query = spec.ThenBy != null ? orderedQuery.ThenBy(spec.ThenBy) : orderedQuery;
            }
            if (spec.OrderByDesc is not null)
            {
                var orderedQuery = query.OrderByDescending(spec.OrderByDesc);
                query = spec.ThenByDesc != null ? orderedQuery.ThenByDescending(spec.ThenByDesc) : orderedQuery;
            }
            if (spec.IsPaginated) query = query.Skip(spec.Skip).Take(spec.Take);
            return query;
        }
    }
}
