#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/OrderBot/OrderBot.MessageProcessors.csproj", "src/OrderBot/"]
COPY ["src/OrderBot.Core/OrderBot.Core.csproj", "src/OrderBot.Core/"]
RUN dotnet restore "src/OrderBot/OrderBot.MessageProcessors.csproj"
COPY . .
WORKDIR "/src/src/OrderBot"
RUN dotnet build "OrderBot.MessageProcessors.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderBot.MessageProcessors.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderBot.MessageProcessors.dll"]