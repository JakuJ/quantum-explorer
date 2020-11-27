using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseHandler
{
    public interface ICodeDatabaseHandler
    {
        public string SaveCode(string name, string code);

        public (string name, string code) GetCode(string key);
    }
}
