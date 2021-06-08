namespace IdentityCore.Interfaces.UnitOfWork
{
    /// <summary>
    /// The DataContextFactory gives us the possibility of not creating the DataContext until
    /// we know which tenant it should be created for. The TenantConnection property must be set
    /// before calling GetDataContext().
    /// </summary>
    public interface IDataContextFactory
    {
        IDataContext GetDataContext();
    }
}
