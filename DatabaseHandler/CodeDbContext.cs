using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DatabaseHandler
{
    public class CodeDbContext : DbContext
    {
        public CodeDbContext(DbContextOptions<CodeDbContext> options) : base(options) { }

        public virtual DbSet<CodeInformation> CodeInformations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CodeInformation>()
                        .HasData(
                         new CodeInformation() { Id = new Guid("29a1a908-0690-49ee-a1b0-fc214bb277c7"), CodeName = "Code1", Code = "CodeFor1", Example = true, ShareTime = DateTime.Now },
                         new CodeInformation() { Id = new Guid("e09144d9-cb21-4461-938b-f8d4e1feaa20"), CodeName = "Code2", Code = "CodeFor2", Example = true, ShareTime = DateTime.Now }
                         );

        }

    }

}
