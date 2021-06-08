using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityInfrastructure.Repositories
{
    public class CrudRepository<TModel> : IRepository<TModel> where TModel : class
    {
        private readonly IDataContext _context;

        public CrudRepository(IDataContextFactory contextFactory)
        {
            _context = contextFactory.GetDataContext();
        }

        public void Add(TModel entity)
        {
            _context.Set<TModel>().Add(entity);
        }

        public void AddRange(IEnumerable<TModel> entities)
        {
            _context.Set<TModel>().AddRange(entities);
        }

        public void Delete(TModel entity)
        {
            _context.Set<TModel>().Remove(entity);
        }

        public void DeleteRange(IEnumerable<TModel> entities)
        {
            _context.Set<TModel>().RemoveRange(entities);
        }

        public void Update(TModel entity)
        {
            _context.Set<TModel>().Update(entity);
        }

        public void UpdateRange(IEnumerable<TModel> entities)
        {
            _context.Set<TModel>().UpdateRange(entities);
        }

        public IQueryable<TModel> Records()
        {
            return _context.Set<TModel>();
        }

    }
}