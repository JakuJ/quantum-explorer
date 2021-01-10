using System.Collections.Generic;
using System.Numerics;
using Common;
using Compiler.AzureFunction;
using Compiler.AzureFunction.Serialization;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class SerializationTest
    {
        private readonly Payload testPayload = new()
        {
            Diagnostics = "Test diagnostics",
            Output = "Some output",
            Grids = new Dictionary<string, List<GateGrid>>
            {
                { "SomeOp", new() { new(3, 3) } },
            },
            States = new List<OperationState>
            {
                new("State1")
                {
                    Arguments = new()
                    {
                        (1, new Complex(5, 3)),
                        (2, new Complex(6, 2)),
                        (3, new Complex(7, 1)),
                    },
                    Children =
                    {
                        new("State2")
                        {
                            Arguments = new()
                            {
                                (1, new Complex(2, 0)),
                                (2, new Complex(0, -1)),
                                (3, new Complex(3, 2)),
                            },
                        },
                    },
                    Results = new()
                    {
                        (0, new Complex(3, -12)),
                        (1, new Complex(8, 0)),
                        (2, new Complex(-2, 5)),
                    },
                },
            },
        };

        [Test]
        public void SerializesAndDeserializesPayloads()
        {
            // Arrange
            JsonSerializerSettings serializationSettings = new()
            {
                ContractResolver = RenamingContractResolver.Instance,
            };

            JsonSerializerSettings deserializationSettings = new()
            {
                MaxDepth = 128,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = RenamingContractResolver.Instance,
                Converters = { ComplexConverter.Instance },
            };

            // Act
            string serialized = JsonConvert.SerializeObject(testPayload, serializationSettings);
            Payload? deserialized = JsonConvert.DeserializeObject<Payload>(serialized, deserializationSettings);

            // Assert
            Assert.IsNotNull(deserialized, "Deserialization should not return null");

            Assert.AreEqual(testPayload.Diagnostics, deserialized!.Diagnostics, "Deserialized diagnostics should be equal to the original");
            Assert.AreEqual(testPayload.Output, deserialized!.Output, "Deserialized output should be equal to the original");
            Assert.AreEqual(testPayload.States, deserialized!.States, "Deserialized states should be equal to the original");
            Assert.AreEqual(testPayload.Grids, deserialized!.Grids, "Deserialized grids should be equal to the original");

            foreach (string key in new[] { "Real", "Imaginary", "Item1", "Item2" })
            {
                Assert.IsFalse(serialized.Contains(key), $"Key {key} should not be present in JSON");
            }
        }
    }
}
