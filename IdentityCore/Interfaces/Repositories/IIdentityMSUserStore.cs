using System.Threading;
using System.Threading.Tasks;
using IdentityCore.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace IdentityCore.Interfaces.Repositories
{
    public interface IIdentityMSUserStore : IUserStore<IdentityMSUser>
    {
        Task<IdentityMSUser> FindByIdAsync(int userId,
            CancellationToken cancellationToken = new CancellationToken());
        Task<IList<IdentityMSUser>> FindByTenantIdAsync(int tenantId,
            CancellationToken cancellationToken = new CancellationToken());
        //Task<IdentityMSUser> FindByIdWithModuleAccessAsync(int userId,
        //    CancellationToken cancellationToken = new CancellationToken());

    }
}

