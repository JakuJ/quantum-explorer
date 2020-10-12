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

# Publish endpoint
FROM build AS publish
RUN dotnet publish -c release --no-build -o /app

# Final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Explorer.dll"]
