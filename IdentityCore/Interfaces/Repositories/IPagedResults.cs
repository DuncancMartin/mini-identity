using System.Collections.Generic;

namespace IdentityCore.Interfaces.Repositories
{
    public interface IPagedResults<T>
    {
        /// <summary>
        /// The number of this page.
        /// </summary>
        int PageNumber { get; set; }

        /// <summary>
        /// The size of this page.
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// The total number of pages available.
        /// </summary>
        int TotalNumberOfPages { get; set; }

        /// <summary>
        /// The total number of records available.
        /// </summary>
        long TotalNumberOfRecords { get; set; }

        /// <summary>
        /// The results of this page.
        /// </summary>
        IEnumerable<T> Results { get; set; }
    }
}