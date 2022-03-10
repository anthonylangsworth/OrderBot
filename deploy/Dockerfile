FROM mcr.microsoft.com/dotnet/sdk:6.0 AS BUILD
WORKDIR /source
COPY ../src/EddnMessageSink/bin/Debug/net6.0/. .
RUN dotnet run EddnMessageSink.exe


FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /home/app
COPY ../src/EddnMessageSink/bin/Debug/net6.0/. .
RUN dotnet run EddnMessageSink.exe
