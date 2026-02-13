using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Contracts
{
    public interface IGenericRepository<T>
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Delete(T entity);
        void Update(T entity);
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(ISpecification<T> specification);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(ISpecification<T>specification);
        Task<int> CountAsync(ISpecification<T> specification);
    }
}
