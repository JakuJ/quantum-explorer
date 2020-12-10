FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

# Install .NET Core 3.1 SDK
ENV DOTNET_NOLOGO=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
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
COPY Compiler.AzureFunction/*.csproj Compiler.AzureFunction/
COPY Common/*.csproj Common/
COPY DatabaseHandler/*.csproj DatabaseHandler/
COPY Simulator/*.csproj Simulator/
COPY AstTransformations/*.fsproj AstTransformations/
RUN dotnet restore Explorer/Explorer.csproj

# Install npm
RUN curl -sL https://deb.nodesource.com/setup_15.x | bash - && \
    apt-get install -y nodejs

# Set node environment
ARG NODE_ENV=development
ENV NODE_ENV=${NODE_ENV}

# Build the app
COPY Explorer/ Explorer/
COPY Compiler/ Compiler/
COPY Compiler.AzureFunction/ Compiler.AzureFunction/
COPY Common/ Common/
COPY DatabaseHandler/ DatabaseHandler/
COPY Simulator/ Simulator/
COPY AstTransformations/ AstTransformations/
WORKDIR Explorer
RUN dotnet build -c Release --no-restore

# Publish the app
FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

# Install libgomp required by Q# at run time
RUN apk add --update libgomp

# Final image
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Explorer.dll"]
