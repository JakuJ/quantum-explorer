# quantum-explorer

# Requirements

Make sure you have the following installed:

- node.js
- a C# compiler supporting .NET Core

# Building

Open the solution in your favourite IDE and build or run the project.
You can also use the `dotnet` utility:

```shell
# build
dotnet build
    
# run at localhost:5001 
dotnet run --project Explorer
```

# Coding style
This repository uses a Roslyn-based analyzer called [StyleCop](https://github.com/DotNetAnalyzers/StyleCopAnalyzers).
Configuration of style enforcement rules and their severities can be done by modifying the [Ruleset](Custom.ruleset) and [config](stylecop.json) files
