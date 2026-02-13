using DomainLayer.Contracts;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class UnitOfWorkRepository(EventReservationDbcontext _reservationDbcontext) : IUnitOfWorkRepository 
    {
        private readonly Dictionary<string, object> AlreadyExistObject = [];
        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            string RepositoryName = typeof(T).Name;
            if (AlreadyExistObject.ContainsKey(RepositoryName))
                return (IGenericRepository<T>)AlreadyExistObject[RepositoryName];
            var NewObject = new GenericRepository<T>(_reservationDbcontext);
            AlreadyExistObject[RepositoryName] = NewObject;
            return NewObject;
        }

        public async Task<int> CompleteAsync() => await _reservationDbcontext.SaveChangesAsync();
    }
}
