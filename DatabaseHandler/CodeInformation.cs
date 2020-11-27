using System;
using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler
{
    /// <summary>
    /// <see cref="CodeInformation"/> represents a record in database.
    /// </summary>
    public class CodeInformation
    {
        public Guid Id { get; set; }

        public bool Example { get; set; }

        public string CodeName { get; set; }

        public DateTime ShareTime { get; set; }

        public string Code { get; set; }
    }
}
