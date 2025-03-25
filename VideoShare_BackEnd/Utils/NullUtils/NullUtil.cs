namespace VideoShare_BackEnd.Utils.NullUtils
{
    public static class NullUtilities
    {
        /// <summary>
        /// Checks whether <paramref name="enumerable" /> is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="enumerable" />.</typeparam>
        /// <param name="enumerable">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to be checked.</param>
        /// <returns>True if <paramref name="enumerable" /> is null or empty, false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any<T>();
        }

    }

}
