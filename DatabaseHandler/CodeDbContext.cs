using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseHandler
{
    public class CodeDbContext: DbContext
    {
        public CodeDbContext(DbContextOptions<CodeDbContext> options) : base(options) { }

        public virtual DbSet<CodeInformation> CodeInformations { get; set; }
    }
}
