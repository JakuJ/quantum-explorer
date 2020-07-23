namespace Explorer.Utilities
{
    public class UniqueId
    {
        private static long counter = 1;
        public static string GetUniqueId() => $"id{counter++}";
    }
}
