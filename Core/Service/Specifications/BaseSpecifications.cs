using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Contracts;
using DomainLayer.Models;

namespace Service.Specifications
{
    public abstract class BaseSpecifications<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>>? WhereExpression { get; }
        public Expression<Func<T, object>> OrderBy { get; private set; }
        public Expression<Func<T, object>> OrderByDesc { get; private set; }
        public Expression<Func<T, object>> ThenBy { get; private set; }
        public Expression<Func<T, object>> ThenByDesc { get; private set; }
        public List<Expression<Func<T, object>>> IncludeExpression { get; } = [];
        public List<Func<IQueryable<T>, IQueryable<T>>> CustomQuerys { get; } = [];
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPaginated { get; private set; }

        protected BaseSpecifications(Expression<Func<T, bool>>? _whereExpression)
        {
            WhereExpression = _whereExpression;
        }

        public void AddOrderBy(Expression<Func<T, object>> expr) => OrderBy = expr;
        public void AddOrderByDesc(Expression<Func<T, object>> expr) => OrderByDesc = expr;

        public void AddThenBy(Expression<Func<T, object>> expr) => ThenBy = expr;
        public void AddThenByDesc(Expression<Func<T, object>> expr) => ThenByDesc = expr;

        public void AddIncludeExpression(Expression<Func<T, object>> expr) => IncludeExpression.Add(expr);

        public void AddCustomQuery(Func<IQueryable<T>, IQueryable<T>> expr) => CustomQuerys.Add(expr);
        

        public void ApplyPagination(int elementsPerPage, int desiredPageIndx)
        {
            IsPaginated = true;
            Take = elementsPerPage;
            var page = desiredPageIndx < 1 ? 1 : desiredPageIndx;
            Skip = (page - 1) * elementsPerPage;
        }
    }
}
