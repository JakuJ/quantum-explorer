using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseHandler
{
    public interface ICodeDatabaseHandler
    {
        /// <summary>
        /// Retrieves code from the databse
        /// </summary>
        /// <param name="key"><see cref="Guid"/> key of the code in database.</param>
        /// <returns>Name and content of the code in database.</returns>
        public Guid SaveCode(string name, string code);

        /// <summary>
        /// Saves code to database.
        /// </summary>
        /// <param name="name">Name of the saved code.</param>
        /// <param name="code">Saved code.</param>
        /// <returns><see cref="Guid"/> key of the code in database.</returns>
        public (string name, string code) GetCode(Guid key);

        /// <summary>
        /// Gets all codes marked as examples in database.
        /// </summary>
        /// <returns>List of codes and their names.</returns>
        public List<(string name, string code)> GetExamples();
    }
}
