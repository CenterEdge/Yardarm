ARG VERSION=0.1.0-local

# --platform=$BUILDPLATFORM ensures that the build runs on the actual CPU platform of the builder, without emulation
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG VERSION
ARG TARGETARCH
WORKDIR /app

# Place a properly formatted RID in /tmp/arch
RUN arch=$TARGETARCH \
    && if [ "$TARGETARCH" = "amd64" ]; then arch="x64"; fi \
    && echo "linux-$arch" > /tmp/arch

COPY src/main/Yardarm/*.csproj ./main/Yardarm/
COPY src/main/Yardarm.Client/*.csproj ./main/Yardarm.Client/
COPY src/main/Yardarm.CommandLine/*.csproj ./main/Yardarm.CommandLine/
COPY src/main/Yardarm.MicrosoftExtensionsHttp/*.csproj ./main/Yardarm.MicrosoftExtensionsHttp/
COPY src/main/Yardarm.MicrosoftExtensionsHttp.Client/*.csproj ./main/Yardarm.MicrosoftExtensionsHttp.Client/
COPY src/main/Yardarm.NewtonsoftJson/*.csproj ./main/Yardarm.NewtonsoftJson/
COPY src/main/Yardarm.NewtonsoftJson.Client/*.csproj ./main/Yardarm.NewtonsoftJson.Client/
COPY src/main/Yardarm.SystemTextJson/*.csproj ./main/Yardarm.SystemTextJson/
COPY src/main/Yardarm.SystemTextJson.Client/*.csproj ./main/Yardarm.SystemTextJson.Client/
COPY ["src/*.props", "src/*.targets", "src/*.snk", "src/nuget.config", "./"]
COPY ["src/main/*.props", "src/main/*.targets", "./main/"]
RUN dotnet restore -r $(cat /tmp/arch) -p:PublishReadyToRun=true ./main/Yardarm.CommandLine/Yardarm.CommandLine.csproj

COPY ./src ./
RUN dotnet publish --no-restore -c Release -r $(cat /tmp/arch) -p:PublishReadyToRun=true -p:VERSION=${VERSION} -o /publish ./main/Yardarm.CommandLine/Yardarm.CommandLine.csproj

# No --platform here so we get the base image for the target platform
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
