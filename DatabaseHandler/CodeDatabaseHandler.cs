using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseHandler
{
    public class CodeDatabaseHandler : ICodeDatabaseHandler
    {
        private readonly CodeDbContext context;
        public CodeDatabaseHandler(CodeDbContext ctx)
        {
            context = ctx;
        }
        public (string name, string code) GetCode(Guid key)
        {
            var codeInfo = context.CodeInformations.FirstOrDefault(c => c.Id == key);
            if(codeInfo == null)
            {
                throw new KeyNotFoundException("Could not find code matching given key");
            }
            return (codeInfo.CodeName, codeInfo.Code);
        }

        public Guid SaveCode(string name, string code)
        {
            CodeInformation codeInformation = new() { CodeName = name, Code = code, ShareTime = DateTime.Now };
            context.CodeInformations.Add(codeInformation);
            context.SaveChanges();

            return codeInformation.Id;
        }
    }
}
