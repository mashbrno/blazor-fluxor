FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview6-bionic AS build
WORKDIR /src
COPY . .

RUN dotnet build "src/Blazor.Fluxor.sln" -c Release