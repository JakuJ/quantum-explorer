using Microsoft.EntityFrameworkCore;

namespace DatabaseHandler
{
    /// <summary>
    /// Representation of a session with the code database.
    /// </summary>
    public class CodeDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeDbContext"/> class.
        /// </summary>
        /// <param name="options">Options for creating the context.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CodeDbContext(DbContextOptions<CodeDbContext> options) : base(options) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets <see cref="DbSet{CodeInformation}"/> representing code table in the database.
        /// </summary>
        public virtual DbSet<CodeInformation> CodeInformations { get; set; }

    }

}
