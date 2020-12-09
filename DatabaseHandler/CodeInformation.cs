using System;
using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler
{
    /// <summary>
    /// Represents a record in a database.
    /// </summary>
    public class CodeInformation
    {
        /// <summary>
        /// Gets or sets id of the record.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets name of the code.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string CodeName { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets time of sharing the code.
        /// </summary>
        public DateTime ShareTime { get; set; }

        /// <summary>
        /// Gets or sets saved code.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Code { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
