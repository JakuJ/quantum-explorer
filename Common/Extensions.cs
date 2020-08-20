using System;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Extension methods used in other projects.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Waits for the task to complete, unwrapping any exceptions from a possible <see cref="AggregateException"/>.
        /// This method allows to use async functions in non-async scopes.
        /// </summary>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        public static void WaitAndUnwrapException(this Task task) => task.GetAwaiter().GetResult();

        /// <inheritdoc cref="WaitAndUnwrapException"/>
        /// <typeparam name="T">The type of the awaited value.</typeparam>
        /// <returns>The value of the awaited task.</returns>
        public static T WaitAndUnwrapException<T>(this Task<T> task) => task.GetAwaiter().GetResult();
    }
}
