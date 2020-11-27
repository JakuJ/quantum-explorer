using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseHandler.Tests
{
    public class CodeDatabaseHandlerTests
    {
        public CodeDbContext GetCodeDbContext()
        {
            var options = new DbContextOptionsBuilder<CodeDbContext>()
                            .UseInMemoryDatabase(databaseName: "InMemoryCodeDatabase")
                            .Options;
            var dbContext = new CodeDbContext(options);

            return dbContext;
        }

        [Test]
        public void GetsExistingCode()
        {
            // Arrange
            CodeDbContext dbContext = GetCodeDbContext();
            Guid codeId = Guid.NewGuid();
            string code = "Test code";
            string name = "Test name";
            CodeInformation codeRecord = new() { Id = codeId, Code = code, CodeName = name, ShareTime = DateTime.Now, Example = false };
            dbContext.CodeInformations.Add(codeRecord);
            dbContext.SaveChanges();

            CodeDatabaseHandler databaseHandler = new(dbContext);

            // Act
            (var resultName, var resultCode) = databaseHandler.GetCode(codeId);

            // Assert
            Assert.AreEqual(resultCode, code, "Code in database and returned code should be equal");
            Assert.AreEqual(resultName, name, "Code in database and returned code should be equal");
        }

        [Test]
        public void ThrowsExceptionWhenNonExistingCode()
        {
            // Arrange
            CodeDbContext dbContext = GetCodeDbContext();
            CodeInformation codeRecord = new() { Id = Guid.NewGuid(), Code = "Test code", CodeName = "Test name", ShareTime = DateTime.Now, Example = false };
            dbContext.CodeInformations.Add(codeRecord);
            dbContext.SaveChanges();

            Guid fakeId = Guid.NewGuid();

            CodeDatabaseHandler databaseHandler = new(dbContext);

            // Act
            // Assert
            Assert.Throws<KeyNotFoundException>(() => databaseHandler.GetCode(fakeId));
        }

        [Test]
        public void SavesCode()
        {
            // Arrange
            CodeDbContext dbContext = GetCodeDbContext();

            string code = "TestCodeTestCode";
            string name = "RandomName";

            CodeDatabaseHandler databaseHandler = new(dbContext);

            // Act
            Guid newId = databaseHandler.SaveCode(name, code);

            // Assert
            Assert.AreEqual(1, dbContext.CodeInformations.Count());
            var savedCode = dbContext.CodeInformations.FirstOrDefault(x => x.Id == newId);
            Assert.NotNull(savedCode);
            Assert.AreEqual(code, savedCode?.Code);
            Assert.AreEqual(name, savedCode?.CodeName);
            Assert.AreEqual(false, savedCode?.Example);
        }
    }
}
