using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public static class OpenIddictEntityFrameworkCoreHelpers
    {

#if SUPPORTS_BCL_ASYNC_ENUMERABLE
        /// <summary>
        /// Executes the query and returns the results as a streamed async enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the returned entities.</typeparam>
        /// <param name="source">The query source.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>The non-streamed async enumeration containing the results.</returns>
#else
        /// <summary>
        /// Executes the query and returns the results as a non-streamed async enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the returned entities.</typeparam>
        /// <param name="source">The query source.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>The non-streamed async enumeration containing the results.</returns>
#endif
        internal static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync(source, cancellationToken);

            static async IAsyncEnumerable<T> ExecuteAsync(IQueryable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken)
            {
#if SUPPORTS_BCL_ASYNC_ENUMERABLE
                await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
                {
                    yield return element;
                }
#else
                foreach (var element in await source.ToListAsync(cancellationToken))
                {
                    yield return element;
                }
#endif
            }
        }
    }
}
