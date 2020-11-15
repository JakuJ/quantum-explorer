FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

# Install npm
RUN apk add --update npm

# Restore dependencies
WORKDIR /source
COPY msbuild/ msbuild/
COPY Explorer/*.csproj Explorer/
COPY Compiler/*.csproj Compiler/
COPY Common/*.csproj Common/
COPY AstTransformations/*.fsproj AstTransformations/
RUN dotnet restore Explorer/Explorer.csproj

# Build the app
COPY Explorer/ Explorer/
COPY Compiler/ Compiler/
COPY Common/ Common/
COPY AstTransformations/ AstTransformations/
WORKDIR Explorer
RUN dotnet build -c release --no-restore

# Publish the app
FROM build AS publish
RUN dotnet publish -c release --no-build -o /app

# Install libgomp required by Q# at run time
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
RUN apk add --update libgomp

# Final image
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Explorer.dll"]
