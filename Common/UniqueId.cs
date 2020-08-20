using System;

namespace Common
{
    /// <summary>
    /// A class allowing for creation of unique IDs that are safe to use as the HTML 'id' properties.
    /// </summary>
    public static class UniqueId
    {
        /// <summary>
        /// Returns a new, unique ID.
        /// Due to use in HTML element identifiers, these IDs always start with a character.
        /// </summary>
        /// <returns>A string of 32 hexadecimal digits that starts with a character 'f'.</returns>
        public static string CreateUniqueId()
        {
            var guid = Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();
            bytes[3] |= 0xF0;
            return new Guid(bytes).ToString("N");
        }
    }
}
