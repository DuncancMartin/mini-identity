using System.Threading.Tasks;
using System.Transactions;

namespace IdentityCore.Interfaces.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
        int SaveChanges();
        TransactionScope Begin();
    }
}
