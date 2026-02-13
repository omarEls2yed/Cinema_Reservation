using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Contracts
{
    public interface ISpecification<T>
    {
        public Expression<Func<T, bool>>? WhereExpression { get; }
        public Expression<Func<T, object>> OrderBy { get; }
        public Expression<Func<T, object>> OrderByDesc { get; }
        public Expression<Func<T, object>> ThenBy { get; }
        public Expression<Func<T, object>> ThenByDesc { get; }
        public List<Expression<Func<T, object>>> IncludeExpression { get; }
        public List<Func<IQueryable<T>, IQueryable<T>>> CustomQuerys { get; }
        public int Take { get; }
        public int Skip { get; }
        public bool IsPaginated { get; }
    }
}
