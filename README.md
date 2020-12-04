# Quantum Explorer

![.NET Core](https://github.com/JakuJ/quantum-explorer/workflows/.NET%20Core/badge.svg)
[![codecov](https://codecov.io/gh/JakuJ/quantum-explorer/branch/develop/graph/badge.svg?token=D74R7H1V3O)](https://codecov.io/gh/JakuJ/quantum-explorer)

This repository holds the code behind the Quantum Explorer, an interactive Q# playground.

# Building

## Requirements

Make sure you have the following installed:

- .NET 5 SDK and runtime
- .NET Core 3.1 SDK and runtime
- npm
- Docker (optional)

## Main app

Use the `dotnet` CLI:

```shell
# build
dotnet build
    
# run
dotnet run --project Explorer

# test
dotnet test
```

# Deployment

We use two versions of the app deployed at all times to the cloud(s).

## Development

The development version of the app, tracking the `develop` branch is running at https://qexplorer.herokuapp.com. The
deployment happens automatically using a push hook configured in Heroku.

The runtime config involves setting **config vars** as follows:

| Variable | Value |
|---|---|
| ASPNETCORE_ENVIRONMENT | Development |
| LANGUAGE_SERVER_URL | wss://qexplorer-ls.herokuapp.com |

This version of the application executes Q# compilation and simulation on the same server as the app itself.

## Production

The production version of the app is deployed to an Azure Web App for Containers running
at https://quantum-explorer.azurewebsites.net.

The deployment is performed manually by pushing the Docker container (or rather, its build context, the build is remote) to Azure Container Repository (ACR).
Run the following Azure CLI commands from the root folder of the repository:

```shell 
az acr build --registry QuantumExplorer --image explorer:<tag> --build-arg NODE_ENV=production --verbose .
```

where `<tag>` is the docker image tag for the release (e.g. `latest` or `1.0`).

The runtime config involves setting **application settings** as follows:

| Variable | Value |
|---|---|
| ASPNETCORE_ENVIRONMENT | Production |
| LANGUAGE_SERVER_URL | wss://language-server.azurewebsites.net  |
| FUNCTION_ENDPOINT | https://qs-compiler.azurewebsites.net/api/CompilerFunction |
| FUNCTION_KEY | \<default key from the Azure Function "App keys" panel> |

This version of the application executes Q# compilation and simulation using the Azure Function from
the `Compiler.AzureFunction` project.

In order to access the production endpoint, the `FUNCTION_KEY` environment variable has to be provided.
Otherwise, the `401 Unauthorized` status code is returned. 

### Azure Functions app

The Q# compiler module used in production is deployed to Azure Functions.
In order to push a new version of the app, open this repository in VS Code with the Azure Functions extension installed.
Rebuild the `Compiler.AzureFunction` project and deploy from the extension to the `qs-compiler` Azure Functions app.

# Notes

## Code style

This repository uses a Roslyn-based analyzer called [StyleCop](https://github.com/DotNetAnalyzers/StyleCopAnalyzers).
Configuration of style enforcement rules and their severities can be done by modifying
the [Ruleset](msbuild/Common.ruleset) and [config](msbuild/stylecop.json) files.
