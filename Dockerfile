FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /source
RUN apk add --update npm

# Copy csproj and fsproj and restore as distinct layers
COPY msbuild/ msbuild/
COPY Explorer/*.csproj Explorer/
COPY Compiler/*.csproj Compiler/
COPY Common/*.csproj Common/
COPY AstTransformations/*.fsproj AstTransformations/

RUN dotnet restore Explorer/Explorer.csproj

# Copy and build app and libraries
COPY Explorer/ Explorer/
COPY Compiler/ Compiler/
COPY Common/ Common/
COPY AstTransformations/ AstTransformations/

WORKDIR /source/Explorer
RUN dotnet build -c release --no-restore

# Test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
WORKDIR /source/tests

COPY Explorer.Tests/ ./Explorer.Tests
COPY Compiler.Tests/ ./Compiler.Tests
COPY Common.Tests/ ./Common.Tests

ENTRYPOINT dotnet test --logger:trx

# Publish endpoint
FROM build AS publish
RUN dotnet publish -c release --no-build -o /app

# Final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80
ENTRYPOINT dotnet Explorer.dll
