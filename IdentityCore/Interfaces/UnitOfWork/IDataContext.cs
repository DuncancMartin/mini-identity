using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IdentityCore.Interfaces.UnitOfWork
{
    public interface IDataContext : IDisposable
    {
        //
        // Summary:
        //     Provides access to information and operations for entity instances this context
        //     is tracking.
        ChangeTracker ChangeTracker { get; }

        //
        // Summary:
        //     Provides access to database related information and operations for this context.
        DatabaseFacade Database { get; }

        //
        // Summary:
        //     Gets an Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry`1 for the given
        //     entity. The entry provides access to change tracking information and operations
        //     for the entity.
        //
        // Parameters:
        //   entity:
        //     The entity to get the entry for.
        //
        // Type parameters:
        //   TEntity:
        //     The type of the entity.
        //
        // Returns:
        //     The entry for the given entity.
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        //
        // Summary:
        //     Saves all changes made in this context to the database.
        //
        // Returns:
        //     The number of state entries written to the database.
        //
        // Remarks:
        //     This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        //     to discover any changes to entity instances before saving to the underlying database.
        //     This can be disabled via Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled.
        int SaveChanges();


        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        //
        // Summary:
        //     Asynchronously saves all changes made in this context to the database.
        //
        // Parameters:
        //   acceptAllChangesOnSuccess:
        //     Indicates whether Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges
        //     is called after the changes have been sent successfully to the database.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     A task that represents the asynchronous save operation. The task result contains
        //     the number of state entries written to the database.
        //
        // Remarks:
        //     This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        //     to discover any changes to entity instances before saving to the underlying database.
        //     This can be disabled via Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled.
        //     Multiple active operations on the same context instance are not supported. Use
        //     'await' to ensure that any asynchronous operations have completed before calling
        //     another method on this context.
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken));

        //
        // Summary:
        //     Creates a Microsoft.EntityFrameworkCore.DbSet`1 that can be used to query and
        //     save instances of TEntity.
        //
        // Type parameters:
        //   TEntity:
        //     The type of entity for which a set should be returned.
        //
        // Returns:
        //     A set for the given entity type.
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

    }
}