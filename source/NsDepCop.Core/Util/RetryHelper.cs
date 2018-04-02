using System;
using OneOf;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// Represents a method called before a retry.
    /// </summary>
    /// <param name="lastException">The exception thrown for the last try.</param>
    public delegate void BeforeRetryCallback(Exception lastException);

    /// <summary>
    /// Helper for retrying an action.
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// Invokes a function and makes a number of retries if it throws an exception.
        /// Invokes a callback for failed tries.
        /// </summary>
        /// <typeparam name="TResult">The return type of the function to invoke.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="maxRetryCount">The maximum number of retries to perform.</param>
        /// <param name="beforeRetryAction">The callback to invoke before retries.</param>
        /// <returns>Either the result of the function or the exception thrown for the last try.</returns>
        public static OneOf<TResult, Exception> Retry<TResult>(Func<TResult> func, int maxRetryCount, BeforeRetryCallback beforeRetryAction)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (maxRetryCount < 0)
                throw new ArgumentOutOfRangeException($"Invalid {nameof(maxRetryCount)} value: {maxRetryCount}. Must be non-negative.");

            var retryCount = 0;

            while (true)
            {
                try
                {
                    return func.Invoke();
                }
                catch (Exception e)
                {
                    if (retryCount == maxRetryCount)
                        return e;

                    beforeRetryAction?.Invoke(e);
                    retryCount++;
                }
            }
        }
    }
}
