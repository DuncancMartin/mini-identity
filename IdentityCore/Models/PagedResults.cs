using System;
using System.Collections.Generic;
using IdentityCore.Interfaces.Repositories;

namespace IdentityCore.Models
{
    public class PagedResults<T> : IPagedResults<T>
    {
        public PagedResults(int pageNumber, int pageSize, long totalNumberOfRecords, IEnumerable<T> results)
        {
            if (pageSize < 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be zero or greater.");

            if (totalNumberOfRecords < 0)
                throw new ArgumentOutOfRangeException(nameof(totalNumberOfRecords),
                    "The total number of records must be zero or greater.");

            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalNumberOfRecords = totalNumberOfRecords;
            Results = results;

            var totalPages = pageSize > 0 ? totalNumberOfRecords / (double) pageSize : 0;
            TotalNumberOfPages = (int) Math.Ceiling(totalPages);
        }

        /// <summary>
        /// The number of this page.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The size of this page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of pages available.
        /// </summary>
        public int TotalNumberOfPages { get; set; }

        /// <summary>
        /// The total number of records available.
        /// </summary>
        public long TotalNumberOfRecords { get; set; }

        /// <summary>
        /// The results of this page.
        /// </summary>
        public IEnumerable<T> Results { get; set; }
    }
}