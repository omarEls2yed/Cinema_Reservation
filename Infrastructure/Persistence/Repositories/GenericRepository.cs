using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Contracts;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class GenericRepository<T>(EventReservationDbcontext _reservationDbcontext)
        : IGenericRepository<T> where T : class
    {
        public async Task AddAsync(T entity) =>
            await _reservationDbcontext.Set<T>().AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<T> entities) =>
            await _reservationDbcontext.Set<T>().AddRangeAsync(entities);

        public void Delete(T entity) =>
            _reservationDbcontext.Set<T>().Remove(entity);

        public void Update(T entity) =>
            _reservationDbcontext.Set<T>().Update(entity);

        public async Task<T?> GetByIdAsync(int id) =>
            await _reservationDbcontext.Set<T>().FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _reservationDbcontext.Set<T>().ToListAsync();

        public async Task<T?> GetByIdAsync(ISpecification<T> specification) =>
            await SpecificationEvaluator.GetQuery(_reservationDbcontext.Set<T>(), specification).FirstOrDefaultAsync();

        public async Task<IEnumerable<T>> GetAllAsync(ISpecification<T> specification) =>
            await SpecificationEvaluator.GetQuery(_reservationDbcontext.Set<T>(), specification).ToListAsync();

        public async Task<int> CountAsync(ISpecification<T> specification) =>
            await SpecificationEvaluator.GetQuery(_reservationDbcontext.Set<T>(), specification).CountAsync();
    }
}
