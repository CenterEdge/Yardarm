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
RUN dotnet publish --no-restore -c Release -r linux-x64 -p:VERSION=${VERSION} -o /publish ./main/Yardarm.CommandLine/Yardarm.CommandLine.csproj

FROM mcr.microsoft.com/dotnet/runtime:8.0
ARG VERSION
WORKDIR /app

COPY --from=build /publish/ ./
RUN groupadd -g 1000 -r yardarm && useradd --no-log-init -u 1000 -r -g yardarm yardarm && \
    mkdir -p /home/yardarm && \
    chown yardarm:yardarm /home/yardarm && \
    ln -s /app/Yardarm.CommandLine /app/yardarm
ENV HOME=/home/yardarm PATH=/app:${PATH}
USER 1000
CMD ["yardarm"]
