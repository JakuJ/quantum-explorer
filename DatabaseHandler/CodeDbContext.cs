using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DatabaseHandler
{
    public class CodeDbContext: DbContext
    {
        public CodeDbContext(DbContextOptions<CodeDbContext> options) : base(options) { }

        public virtual DbSet<CodeInformation> CodeInformations { get; set; }

    }

}
