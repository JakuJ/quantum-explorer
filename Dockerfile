FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

# Set node environment
ARG NODE_ENV=development
ENV NODE_ENV=${NODE_ENV}

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
RUN dotnet build -c Release --no-restore

# Publish the app
FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app

# Install libgomp required by Q# at run time
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

# Set ASP.NET Core runtime environment
ARG ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}

RUN apk add --update libgomp

# Final image
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Explorer.dll"]
