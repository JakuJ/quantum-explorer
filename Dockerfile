FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

# Install .NET Core 3.1 SDK
RUN wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb; \
    dpkg -i packages-microsoft-prod.deb; \
    apt-get update; \
    apt-get install -y apt-transport-https && \
    apt-get update && \
    apt-get install -y dotnet-sdk-3.1

# Restore dependencies
WORKDIR /source
COPY msbuild/ msbuild/
COPY Explorer/*.csproj Explorer/
COPY Compiler/*.csproj Compiler/
COPY CompilerFunction/*.csproj CompilerFunction/
COPY Common/*.csproj Common/
COPY AstTransformations/*.fsproj AstTransformations/
RUN dotnet restore Explorer/Explorer.csproj

# Install npm
RUN apt install -y npm

# Set node environment
ARG NODE_ENV=development
ENV NODE_ENV=${NODE_ENV}
ARG LS_HOST=localhost
ENV LS_HOST=${LS_HOST}

# Build the app
COPY Explorer/ Explorer/
COPY Compiler/ Compiler/
COPY CompilerFunction/ CompilerFunction/
COPY Common/ Common/
COPY AstTransformations/ AstTransformations/
WORKDIR Explorer
RUN dotnet build -c Release --no-restore

# Publish the app
FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

# Set ASP.NET Core runtime environment
ARG ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}

# Install libgomp required by Q# at run time
RUN apk add --update libgomp

# Final image
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Explorer.dll"]
