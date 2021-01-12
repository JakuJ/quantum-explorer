using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseHandler
{
    /// <inheritdoc cref="ICodeDatabaseHandler"/>
    /// <summary>
    /// Handles saving and retrieving code from the database.
    /// </summary>
    public class CodeDatabaseHandler : ICodeDatabaseHandler
    {
        private readonly CodeDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeDatabaseHandler"/> class.
        /// </summary>
        /// <param name="ctx">Database context.</param>
        public CodeDatabaseHandler(CodeDbContext ctx) => context = ctx;

        /// <inheritdoc/>
        public async Task<bool> CheckConnection() => await context.Database.CanConnectAsync();

        /// <inheritdoc/>
        public (string Name, string Code) GetCode(Guid key)
        {
            CodeInformation? codeInfo = context.CodeInformations.FirstOrDefault(c => c.Id == key);
            if (codeInfo == null)
            {
                throw new KeyNotFoundException("Could not find code matching given key");
            }

            return (codeInfo.CodeName, codeInfo.Code);
        }

        /// <inheritdoc/>
        public Guid SaveCode(string name, string code)
        {
            CodeInformation codeInformation = new() { CodeName = name, Code = code, ShareTime = DateTime.Now };
            context.CodeInformations.Add(codeInformation);
            context.SaveChanges();

            return codeInformation.Id;
        }
    }
}
