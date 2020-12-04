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
            var savedCode = dbContext.CodeInformations.FirstOrDefault(x => x.Id == newId);
            Assert.NotNull(savedCode);
            Assert.AreEqual(code, savedCode?.Code);
            Assert.AreEqual(name, savedCode?.CodeName);
            Assert.AreEqual(false, savedCode?.Example);
        }

        [Test]
        public void ReturnsEmptyListWhenNoExamples()
        {
            // Arrange
            CodeDbContext dbContext = GetCodeDbContext();
            CodeDatabaseHandler databaseHandler = new(dbContext);

            // Act
            var list = databaseHandler.GetExamples();

            // Assert
            Assert.IsEmpty(list);
        }

        [Test]
        public void ReturnsExamplesInDb()
        {
            // Arrange
            CodeDbContext dbContext = GetCodeDbContext();
            dbContext.CodeInformations.AddRange(
                new CodeInformation() { CodeName = "TestName1", Code = "TestCode1", Example = true, ShareTime = DateTime.Now },
                new CodeInformation() { CodeName = "TestName2", Code = "TestCode2", Example = true, ShareTime = DateTime.Now }
            );
            dbContext.SaveChanges();
            CodeDatabaseHandler databaseHandler = new(dbContext);

            // Act
            var list = databaseHandler.GetExamples();

            // Assert
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(("TestName1", "TestCode1"), list[0]);
            Assert.AreEqual(("TestName2", "TestCode2"), list[1]);
        }
    }
}
