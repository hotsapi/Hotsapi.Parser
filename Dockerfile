FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /app

COPY Hotsapi.Parser/*.csproj ./Hotsapi.Parser/
COPY Hotsapi.Parser.Test/*.csproj ./Hotsapi.Parser.Test/
COPY Heroes.ReplayParser/Heroes.ReplayParser/*.csproj ./Heroes.ReplayParser/Heroes.ReplayParser/
COPY Heroes.ReplayParser/MpqTool/*.csproj ./Heroes.ReplayParser/MpqTool/
RUN cd Hotsapi.Parser && dotnet restore

COPY . .
RUN dotnet publish -c Release -r linux-x64 -o out --self-contained true /p:PublishTrimmed=true Hotsapi.Parser
RUN dotnet test --logger:"console;verbosity=normal" -c Debug Hotsapi.Parser.Test/Hotsapi.Parser.Test.csproj


FROM mcr.microsoft.com/dotnet/core/runtime-deps:2.2-stretch-slim AS runtime
RUN apt-get update && apt-get install -y libunwind-dev && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/Hotsapi.Parser/out ./
EXPOSE 8080
ENTRYPOINT ["./Hotsapi.Parser"]

