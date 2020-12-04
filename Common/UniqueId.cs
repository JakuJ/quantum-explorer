namespace Common
{
    /// <summary>
    /// A class allowing for creation of unique IDs that are safe to use as HTML 'id' properties.
    /// </summary>
    public static class UniqueId
    {
        private static long counter;

        /// <summary>
        /// Returns a new, unique ID.
        /// </summary>
        /// <returns>A unique id.</returns>
        public static string CreateUniqueId() => $"_{counter++}";
    }
}
