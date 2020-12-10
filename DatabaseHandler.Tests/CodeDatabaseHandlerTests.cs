using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

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
            CodeInformation codeRecord = new() { Id = codeId, Code = code, CodeName = name, ShareTime = DateTime.Now };
            dbContext.CodeInformations.Add(codeRecord);
            dbContext.SaveChanges();

            CodeDatabaseHandler databaseHandler = new(dbContext);

            // Act
            (var resultName, var resultCode) = databaseHandler.GetCode(codeId);

            // Assert
            Assert.AreEqual(resultCode, code, "Code in database and returned code should be equal");
            Assert.AreEqual(resultName, name, "Name in database and returned name should be equal");
        }

        [Test]
        public void ThrowsExceptionWhenNonExistingCode()
        {
            // Arrange
            CodeDbContext dbContext = GetCodeDbContext();
            CodeInformation codeRecord = new() { Id = Guid.NewGuid(), Code = "Test code", CodeName = "Test name", ShareTime = DateTime.Now };
            dbContext.CodeInformations.Add(codeRecord);
            dbContext.SaveChanges();

            Guid fakeId = Guid.NewGuid();

            CodeDatabaseHandler databaseHandler = new(dbContext);

            // Act
            // Assert
            Assert.Throws<KeyNotFoundException>(() => databaseHandler.GetCode(fakeId), "Exception should be thrown");
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
            Assert.NotNull(savedCode, "Code in db shouldn't be null");
            Assert.AreEqual(code, savedCode?.Code, "Code in db should be equal to passed value");
            Assert.AreEqual(name, savedCode?.CodeName, "Name of the code should be equal to passed value");
        }

        [Test]
        public void ChecksConnectionTrue()
        {
            // Arrange
            CodeDbContext dbContext = GetCodeDbContext();
            CodeDatabaseHandler databaseHandler = new(dbContext);

            // Act
            bool canConnect = databaseHandler.CheckConnection();

            // Assert
            Assert.IsTrue(canConnect, "Db should be able to connect");
        }

        [Test]
        public void ChecksConnectionFalse()
        {
            // Arrange
            CodeDbContext dbContext = new CodeDbContext(new DbContextOptionsBuilder<CodeDbContext>().UseSqlServer("incorrectConnectionString").Options);
            CodeDatabaseHandler databaseHandler = new(dbContext);

            // Act
            bool canConnect = databaseHandler.CheckConnection();

            // Assert
            Assert.IsFalse(canConnect, "Db should not be able to connect");
        }
    }
}
