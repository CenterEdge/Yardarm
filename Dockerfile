ARG VERSION=0.1.0-local

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG VERSION
WORKDIR /app

COPY src/main/Yardarm/*.csproj ./main/Yardarm/
COPY src/main/Yardarm.Client/*.csproj ./main/Yardarm.Client/
COPY src/main/Yardarm.CommandLine/*.csproj ./main/Yardarm.CommandLine/
COPY src/main/Yardarm.MicrosoftExtensionsHttp/*.csproj ./main/Yardarm.MicrosoftExtensionsHttp/
COPY src/main/Yardarm.MicrosoftExtensionsHttp.Client/*.csproj ./main/Yardarm.MicrosoftExtensionsHttp.Client/
COPY src/main/Yardarm.NewtonsoftJson/*.csproj ./main/Yardarm.NewtonsoftJson/
COPY src/main/Yardarm.NewtonsoftJson.Client/*.csproj ./main/Yardarm.NewtonsoftJson.Client/
COPY src/main/Yardarm.SystemTextJson/*.csproj ./main/Yardarm.SystemTextJson/
COPY src/main/Yardarm.SystemTextJson.Client/*.csproj ./main/Yardarm.SystemTextJson.Client/
COPY ["src/*.props", "src/*.targets", "src/*.snk", "./"]
RUN dotnet restore ./main/Yardarm.CommandLine/Yardarm.CommandLine.csproj

COPY ./src ./
RUN dotnet pack -c Release -p:VERSION=${VERSION} ./main/Yardarm.CommandLine/Yardarm.CommandLine.csproj

FROM mcr.microsoft.com/dotnet/sdk:8.0
ARG VERSION
WORKDIR /app

RUN groupadd -g 1000 -r yardarm && useradd --no-log-init -u 1000 -r -g yardarm yardarm && \
    mkdir -p /home/yardarm && \
    chown yardarm:yardarm /home/yardarm
ENV HOME=/home/yardarm PATH=/home/yardarm/.dotnet/tools:${PATH}
USER yardarm

COPY --from=build /app/main/Yardarm.CommandLine/bin/Release/Yardarm.CommandLine.*.nupkg ./
RUN dotnet tool install --global --add-source /app --version ${VERSION} Yardarm.CommandLine
