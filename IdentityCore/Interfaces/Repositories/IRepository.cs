using System.Collections.Generic;
using System.Linq;

namespace IdentityCore.Interfaces.Repositories
{
    public interface IRepository<TModel>
    {
        IQueryable<TModel> Records();

        void Add(TModel entity);
        void AddRange(IEnumerable<TModel> entities);

        void Delete(TModel entity);
        void DeleteRange(IEnumerable<TModel> entities);

        void Update(TModel entity);
        void UpdateRange(IEnumerable<TModel> entities);
    }
}
