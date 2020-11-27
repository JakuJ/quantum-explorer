# Quantum Explorer
![.NET Core](https://github.com/JakuJ/quantum-explorer/workflows/.NET%20Core/badge.svg)
[![codecov](https://codecov.io/gh/JakuJ/quantum-explorer/branch/develop/graph/badge.svg?token=D74R7H1V3O)](https://codecov.io/gh/JakuJ/quantum-explorer)

This repository holds the code behind the Quantum Explorer, an interactive Q# playground.

# Requirements

Make sure you have the following installed:

- .NET 5 SDK and runtime for the main app
- npm
- Docker
- .NET 3.1 SDK and runtime for the language server (or just use Docker)

# Building

Use the `dotnet` CLI:

```shell
# build
dotnet build
    
# run
dotnet run --project Explorer

# test
dotnet test
```

# Language server

The language server is in a separate repository added as a submodule to this one. For development purposes, the app can be run inside a Docker container.

```shell
# using docker-compose
docker-compose up --build language-server

# or using Docker manually
cd qsharp-compiler-mirror
docker build -t language-server .
docker run -p 8091:8091 -t language-server
```

By default, the app runs on port `8091`.

If you want to build locally, you must first execute the `bootstrap.ps1` script at the root of the `qsharp-compiler-mirror` repository
with the correct value of the `NUGET_VERSION` environment variable (defined in the [Dockerfile](./qsharp-compiler-mirror/Dockerfile) for the language server).

```shell
# On MacOS or Linux, pwsh is the Powershell binary
NUGET_VERSION=0.14.2011120240 pwsh bootstrap.ps1

# On Windows, in Powershell
$Env:NUGET_VERSION=0.14.2011120240
bootstrap.ps1

# build and run the Language Server afterwards
cd src/QsCompiler/ServerRunner
dotnet run
```

# Code style
This repository uses a Roslyn-based analyzer called [StyleCop](https://github.com/DotNetAnalyzers/StyleCopAnalyzers).
Configuration of style enforcement rules and their severities can be done by modifying the [Ruleset](msbuild/Common.ruleset) and [config](msbuild/stylecop.json) files.
