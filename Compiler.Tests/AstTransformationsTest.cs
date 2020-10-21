using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AstTransformations;
using Common;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class AstTransformationsTest
    {
        private static class DeclarationSources
        {
            internal static object[] Sources => new object[]
            {
                new object[] { "Library", new[] { "RandomBit", "RandomInt" } },
                new object[] { "MultipleOperations", new[] { "EntanglePair", "IdentityGate", "NoOp", "RandomBit" } },
                new object[]
                {
                    "AllocatedQubitOps",
                    new[] { "AllocateFive", "AllocateFiveAndApplyGates", "AllocateOne", "AllocateOneAndApplyGates" },
                },
            };
        }

        private static class SourceClass
        {
            public static async Task<Dictionary<string, GateGrid>> Process(string path)
            {
                string code = await Helpers.GetSourceFile(path);
                var compiler = new QsCompiler(Helpers.ConsoleLogger);

                await compiler.Compile(code);
                return FromQSharp.GetGates(compiler.Compilation);
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
                        new (string, int, int)[] { },
                        new[] { "qubit" },
                    };

                    yield return new object[]
                    {
                        "AllocateFive",
                        new (string, int, int)[] { },
                        new[] { "qs[0]", "qs[1]", "qs[2]", "qs[3]", "qs[4]" },
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
                            ("Z", 3, 4),
                        },
                        new[] { "qs[0]", "qs[1]", "qs[2]", "qs[3]", "qs[4]" },
                    };
                }
            }

            internal static Dictionary<string, GateGrid> Grids { get; } = SourceClass.Process("AllocatedQubitOps").WaitAndUnwrapException();
        }

        private static class ArgumentSources
        {
            internal static IEnumerable<object> Sources
            {
                get
                {
                    yield return new object[]
                    {
                        "Op1",
                        new (string, int, int)[] { ("H", 0, 0) },
                        new[] { "target" },
                    };

                    yield return new object[]
                    {
                        "Op2",
                        new (string, int, int)[] { ("H", 0, 1), ("H", 1, 0) },
                        new[] { "target1", "target2" },
                    };

                    yield return new object[]
                    {
                        "OpReg1",
                        new (string, int, int)[] { ("H", 0, 0) },
                        new[] { "targets[0]" },
                    };

                    yield return new object[]
                    {
                        "OpReg2",
                        new (string, int, int)[]
                        {
                            ("H", 0, 0),
                            ("H", 1, 1),
                        },
                        new[] { "targets[0]", "targets[1]" },
                    };

                    yield return new object[]
                    {
                        "OpReg5",
                        new (string, int, int)[]
                        {
                            ("H", 0, 0),
                            ("H", 1, 1),
                            ("H", 2, 2),
                            ("H", 3, 3),
                            ("H", 4, 4),
                        },
                        new[]
                        {
                            "targets[2]",
                            "targets[4]",
                            "targets[1]",
                            "targets[3]",
                            "targets[0]",
                        },
                    };
                }
            }

            internal static Dictionary<string, GateGrid> Grids { get; } = SourceClass.Process("QubitArguments").WaitAndUnwrapException();
        }

        private static class MixedSources
        {
            internal static IEnumerable<object> Sources
            {
                get
                {
                    yield return new object[]
                    {
                        "Mixed1",
                        new (string, int, int)[]
                        {
                            ("X", 0, 0),
                            ("H", 1, 1),
                            ("Y", 2, 2),
                        },
                        new[] { "q", "qs[0]", "qs[3]" },
                    };

                    yield return new object[]
                    {
                        "Mixed2",
                        new (string, int, int)[]
                        {
                            ("H", 0, 0),
                            ("X", 1, 2),
                            ("Z", 2, 1),
                        },
                        new[] { "qs[1]", "qs[2]", "q" },
                    };

                    yield return new object[]
                    {
                        "Mixed3",
                        new (string, int, int)[]
                        {
                            ("X", 0, 0),
                            ("Y", 1, 3),
                            ("X", 2, 1),
                            ("Y", 3, 4),
                            ("X", 4, 2),
                            ("Y", 5, 5),
                        },
                        new[] { "reg1[0]", "reg1[1]", "reg1[2]", "reg2[0]", "reg2[1]", "reg2[2]" },
                    };
                }
            }

            internal static Dictionary<string, GateGrid> Grids { get; } = SourceClass.Process("MixedSources").WaitAndUnwrapException();
        }

        [TestCaseSource(typeof(DeclarationSources), nameof(DeclarationSources.Sources))]
        public async Task ListsDeclaredOperations(string path, string[] operations)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(path);
            var compiler = new QsCompiler(Helpers.ConsoleLogger);
            var qualifiedNames = operations.Select(x => $"{path}.{x}").ToArray();

            // Act
            await compiler.Compile(code);
            var gates = FromQSharp.GetGates(compiler.Compilation);

            // Assert
            Assert.AreEqual(qualifiedNames, gates.Keys.ToArray(), "All declared operations should be listed.");
        }

        [Test]
        public async Task ProcessesMultipleNamespaces()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("MultipleNamespaces");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);
            string[] qualifiedNames = new[] { "Ns1.Op1", "Ns2.Op1", "Ns2.Op2", "Ns3.Op2" };

            // Act
            await compiler.Compile(code);
            var gates = FromQSharp.GetGates(compiler.Compilation);

            // Assert
            Assert.AreEqual(qualifiedNames, gates.Keys.ToArray(), "All declared operations should be listed.");
        }

        [TestCase("EmptyFile")]
        [TestCase("EmptyNamespace")]
        [TestCase("SingleEmptyFunction")]
        public async Task FindsNoOperations(string path)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(path);
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            // Act
            await compiler.Compile(code);
            var gates = FromQSharp.GetGates(compiler.Compilation);

            // Assert
            Assert.IsEmpty(gates, "Traversing the AST should find no operations.");
        }

        [TestCaseSource(typeof(AllocationSources), nameof(AllocationSources.Sources))]
        public void ExtractsQuantumGates(string operation, (string, int, int)[] gates, string[] names)
        {
            // Arrange (& Act)
            var grids = AllocationSources.Grids;
            operation = $"AllocatedQubitOps.{operation}";

            // Assert
            foreach ((QuantumGate gate, int x, int y) in grids[operation].Gates)
            {
                Assert.Contains((gate.Name, x, y), gates, "Gate should be present at a given position");
            }

            Assert.AreEqual(names, grids[operation].Names, "Assigned qubit identifiers should be correct");
        }

        [Test]
        public async Task LocallyDeclaredOperations()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("LocalOps");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);

            // Act
            await compiler.Compile(code);
            GateGrid? grid = FromQSharp.GetGates(compiler.Compilation)["LocalOps.MotherOp"];

            // Assert
            Assert.AreEqual(1, grid.Gates.Count(), "There should be 1 gate in the grid");

            (var gate, int x, int y) = grid.Gates.First();
            Assert.AreEqual(("DoesSomethingWithASingleQubit", 0, 0), (gate.Name, x, y), "A local gate should be detected");
        }

        [TestCaseSource(typeof(ArgumentSources), nameof(ArgumentSources.Sources))]
        public void QubitArguments(string operation, (string, int, int)[] gates, string[] names)
        {
            // Arrange (& Act)
            var grids = ArgumentSources.Grids;
            operation = $"QubitArguments.{operation}";

            // Assert
            foreach ((QuantumGate gate, int x, int y) in grids[operation].Gates)
            {
                Assert.Contains((gate.Name, x, y), gates, "Gate should be present at a given position");
            }

            Assert.AreEqual(names, grids[operation].Names, "Assigned qubit identifiers should be correct");
        }

        [TestCaseSource(typeof(MixedSources), nameof(MixedSources.Sources))]
        public void MixedQubitSources(string operation, (string, int, int)[] gates, string[] names)
        {
            // Arrange (& Act)
            var grids = MixedSources.Grids;
            operation = $"MixedSources.{operation}";

            // Assert
            foreach ((QuantumGate gate, int x, int y) in grids[operation].Gates)
            {
                Assert.Contains((gate.Name, x, y), gates, "Gate should be present at a given position");
            }

            Assert.AreEqual(names, grids[operation].Names, "Assigned qubit identifiers should be correct");
        }

        [Test]
        public async Task MultiArgumentGates()
        {
            // Arrange
            string code = await Helpers.GetSourceFile("Arguments");
            var compiler = new QsCompiler(Helpers.ConsoleLogger);
            var expected = new[]
            {
                ("X", 0, 0, 0, false),

                ("TwoArgs", 1, 0, 0, false),
                ("TwoArgs", 1, 1, 1, false),

                ("TwoArgs", 2, 4, 0, false),
                ("TwoArgs", 3, 4, 1, false),

                ("RegArg", 4, 0, 0, true),
                ("RegArg", 4, 1, 0, true),
                ("RegArg", 4, 3, 0, true),

                ("SingleAndRegArgs", 5, 1, 2, false),
                ("SingleAndRegArgs", 5, 2, 1, true),
                ("SingleAndRegArgs", 5, 3, 1, true),
                ("SingleAndRegArgs", 5, 4, 0, false),
            };

            // Act
            await compiler.Compile(code);
            GateGrid? grid = FromQSharp.GetGates(compiler.Compilation)["Arguments.TestOp"];

            // Assert
            foreach ((QuantumGate gate, int x, int y) in grid.Gates)
            {
                Assert.Contains((gate.Name, x, y, gate.ArgIndex, gate.ArgArray), expected, "Gate should be present at a given position");
            }
        }
    }
}
