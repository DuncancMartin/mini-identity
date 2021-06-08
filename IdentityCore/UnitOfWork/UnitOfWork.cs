using System.Threading.Tasks;
using System.Transactions;
using IdentityCore.Interfaces.UnitOfWork;

namespace IdentityCore.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDataContextFactory _contextFactory;
        private IDataContext Context => _contextFactory.GetDataContext();

        public UnitOfWork(IDataContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task<int> SaveChangesAsync()
        {
            return Context.SaveChangesAsync(true);
        }

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }

        /// <summary>
        /// Creates a TransactionScope to make it possible to consume services from services.
        /// This will created nested transaction scopes which will all do what the outer scope does (commit or rollback)
        /// </summary>
        public TransactionScope Begin()
        {
            // TransactionScopeAsyncFlowOption.Enabled is required to be able to use it with async functions
            return new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}
