ARG VERSION=0.1.0-local

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG VERSION
WORKDIR /app

COPY src/*.sln ./
COPY src/Yardarm/*.csproj ./Yardarm/
COPY src/Yardarm.UnitTests/*.csproj ./Yardarm.UnitTests/
COPY src/Yardarm.Client/*.csproj ./Yardarm.Client/
COPY src/Yardarm.Client.UnitTests/*.csproj ./Yardarm.Client.UnitTests/
COPY src/Yardarm.CommandLine/*.csproj ./Yardarm.CommandLine/
COPY src/Yardarm.NewtonsoftJson/*.csproj ./Yardarm.NewtonsoftJson/
COPY src/Yardarm.NewtonsoftJson.Client/*.csproj ./Yardarm.NewtonsoftJson.Client/
RUN dotnet restore Yardarm.sln

COPY ./src ./
RUN dotnet pack -c Release -p:VERSION=${VERSION} ./Yardarm.CommandLine/Yardarm.CommandLine.csproj

FROM mcr.microsoft.com/dotnet/sdk:5.0
ARG VERSION
WORKDIR /app

RUN groupadd -g 1000 -r yardarm && useradd --no-log-init -u 1000 -r -g yardarm yardarm && \
    mkdir -p /home/yardarm && \
    chown yardarm:yardarm /home/yardarm
ENV HOME=/home/yardarm PATH=/home/yardarm/.dotnet/tools:${PATH}
USER yardarm

COPY --from=build /app/Yardarm.CommandLine/bin/Release/Yardarm.CommandLine.*.nupkg ./
RUN dotnet tool install --global --add-source /app --version ${VERSION} Yardarm.CommandLine
