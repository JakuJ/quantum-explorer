using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseHandler
{
    public interface ICodeDatabaseHandler
    {
        string SaveCode(string name, string code);

        (string name, string code) GetCode(string key);


    }
}
