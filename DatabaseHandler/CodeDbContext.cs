using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
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

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CodeDbContext>
    {
        public CodeDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("./appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<CodeDbContext>();
            var connectionString = configuration.GetConnectionString("DatabaseConnection");
            builder.UseSqlServer(connectionString);
            return new CodeDbContext(builder.Options);
        }
    }
}
