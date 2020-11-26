using System;
using System.ComponentModel.DataAnnotations;

namespace DatabaseHandler
{
    public class CodeInformation
    {
        public Guid Id { get; set; }

        public string CodeName { get; set; }

        public DateTime ShareTime { get; set; }

        public string Code { get; set; }
    }
}
