using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Common;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class OperationGridsTest
    {
        private static class DeclarationSources
        {
            internal static object[] Sources => new object[]
            {
                new object[] { "Library", new[] { "Main", "RandomBit", "RandomInt" } },
                new object[] { "MultipleOperations", new[] { "Main", "EntanglePair", "IdentityGate", "NoOp", "RandomBit" } },
                new object[]
                {
                    "AllocatedQubitOps",
                    new[] { "Main", "AllocateFive", "AllocateFiveAndApplyGates", "AllocateOne", "AllocateOneAndApplyGates", "Tuples" },
                },
            };
        }

        private static class SourceClass
        {
            public static async Task<Dictionary<string, List<GateGrid>>> Process(string path)
            {
                string code = await Helpers.GetSourceFile(path);
                var compiler = new QsCompiler(Helpers.ConsoleLogger);

                Dictionary<string, List<GateGrid>> gates = null!;
                compiler.OnGrids += (_, grids) => { gates = grids; };

                await compiler.Compile(code);
                return gates;
            }
        }

        private static class AllocationSources
        {
            internal static IEnumerable<object> Sources
            {
                get
                {
                    yield return new object[]
                    {
                        "AllocateOne",
                        Array.Empty<(string, int, int)>(),
                        Array.Empty<string>(),
                    };

                    yield return new object[]
                    {
                        "AllocateFive",
                        Array.Empty<(string, int, int)>(),
                        Array.Empty<string>(),
                    };

                    yield return new object[]
                    {
                        "AllocateOneAndApplyGates",
                        new[]
                        {
                            ("H", 0, 0),
                            ("Z", 1, 0),
                            ("X", 2, 0),
                            ("MResetZ", 3, 0),
                        },
                        new[] { "q" },
                    };
                    yield return new object[]
                    {
                        "AllocateFiveAndApplyGates",
                        new[]
                        {
                            ("H", 0, 0),
                            ("X", 1, 1),
                            ("Y", 2, 1),
                            ("Z", 3, 2),
                        },
                        new[] { "qs[0]", "qs[1]", "qs[4]" },
                    };
                    yield return new object[]
                    {
                        "Tuples",
                        new[]
                        {
                            ("X", 0, 0),
                            ("Y", 1, 2),
                            ("H", 2, 1),
                        },
                        new[] { "q1", "q2", "qs[0]" },
                    };
                }
            }

            internal static Dictionary<string, List<GateGrid>> Grids { get; } = SourceClass.Process("AllocatedQubitOps").WaitAndUnwrapException();
        }

        [TestCaseSource(typeof(DeclarationSources), nameof(DeclarationSources.Sources))]
        public async Task ListsDeclaredOperations(string path, string[] operations)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(path);
            QsCompiler compiler = new(Helpers.ConsoleLogger);
            string[] qualifiedNames = operations.Select(x => $"{path}.{x}").ToArray();

            Dictionary<string, List<GateGrid>> gates = null!;
            compiler.OnGrids += (_, grids) => { gates = grids; };

            // Act
            await compiler.Compile(code);

            // Assert
            Assert.AreEqual(qualifiedNames.ToImmutableSortedSet(), gates.Keys.ToImmutableSortedSet(), "All declared operations should be listed.");
        }

        [Test]
        public async Task ProcessesMultipleNamespaces()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("MultipleNamespaces");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);
            string[] qualifiedNames = { "Ns1.Op1", "Ns2.Op1", "Ns2.Op2", "Ns3.Op2" };

            Dictionary<string, List<GateGrid>>? gates = null;
            compiler.OnGrids += (_, grids) => { gates = grids; };

            // Act
            await compiler.Compile(code);

            // Assert
            Assert.AreEqual(qualifiedNames.ToImmutableSortedSet(), gates?.Keys.ToImmutableSortedSet(), "All declared operations should be listed.");
        }

        [TestCase("EmptyFile")]
        [TestCase("EmptyNamespace")]
        [TestCase("SingleEmptyFunction")]
        public async Task FindsNoOperations(string path)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(path);
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            compiler.OnGrids += (_, _) => Assert.Fail("No grids should be returned.");

            // Act && Assert
            await compiler.Compile(code);
        }

        [TestCaseSource(typeof(AllocationSources), nameof(AllocationSources.Sources))]
        public void ProcessesAllocations(string operation, (string, int, int)[] gates, string[] names)
        {
            // Arrange (& Act)
            var grids = AllocationSources.Grids;
            operation = $"AllocatedQubitOps.{operation}";

            // Assert
            foreach ((QuantumGate gate, int x, int y) in grids[operation].Single().Gates)
            {
                Assert.Contains((gate.Name, x, y), gates, "Gate should be present at a given position");
            }

            Assert.AreEqual(names, grids[operation].Single().Names, "Assigned qubit identifiers should be correct");
        }

        [Test]
        public async Task LocallyDeclaredOperations()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("LocalOps");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            GateGrid grid = null!;
            compiler.OnGrids += (_, grids) => { grid = grids["LocalOps.MotherOp"].Single(); };

            // Act
            await compiler.Compile(code);

            // Assert
            Assert.AreEqual(1, grid.Gates.Count(), "There should be 1 gate in the grid");

            (var gate, int x, int y) = grid.Gates.First();
            Assert.AreEqual(("DoesSomethingWithASingleQubit", 0, 0), (gate.Name, x, y), "A local gate should be detected");
        }

        [Test]
        public async Task MultiArgumentGates()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("Arguments");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);
            var expected = new[]
            {
                ("X", 0, 0, 0),

                ("TwoArgs", 1, 0, 0),
                ("TwoArgs", 1, 1, 1),

                ("TwoArgs", 2, 4, 0),
                ("TwoArgs", 3, 4, 1),

                ("RegArg", 4, 1, 0),
                ("RegArg", 4, 0, 1),
                ("RegArg", 4, 3, 2),

                ("SingleAndRegArgs", 5, 4, 0),
                ("SingleAndRegArgs", 5, 2, 1),
                ("SingleAndRegArgs", 5, 3, 2),
                ("SingleAndRegArgs", 5, 1, 3),
            };

            GateGrid grid = null!;
            compiler.OnGrids += (_, grids) => { grid = grids["Arguments.TestOp"].Single(); };

            // Act
            await compiler.Compile(code);

            // Assert
            foreach ((QuantumGate gate, int x, int y) in grid.Gates)
            {
                Assert.Contains((gate.Name, x, y, gate.ArgIndex), expected, "Gate should be present at a given position");
            }
        }
    }
}
