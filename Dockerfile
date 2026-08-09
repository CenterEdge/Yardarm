ARG VERSION=0.1.0-local

# --platform=$BUILDPLATFORM ensures that the build runs on the actual CPU platform of the builder, without emulation
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION
ARG TARGETARCH
WORKDIR /build

# Place a properly formatted RID in /tmp/arch
RUN arch=$TARGETARCH \
    && if [ "$TARGETARCH" = "amd64" ]; then arch="x64"; fi \
    && echo "linux-$arch" > /tmp/arch

COPY src/main/Core/Yardarm/*.csproj ./main/Core/Yardarm/
COPY src/main/Core/Yardarm.Client/*.csproj ./main/Core/Yardarm.Client/
COPY src/main/Core/Yardarm.CommandLine/*.csproj ./main/Core/Yardarm.CommandLine/
COPY src/main/MicrosoftExtensionsHttp/Yardarm.MicrosoftExtensionsHttp/*.csproj ./main/MicrosoftExtensionsHttp/Yardarm.MicrosoftExtensionsHttp/
COPY src/main/MicrosoftExtensionsHttp/Yardarm.MicrosoftExtensionsHttp.Client/*.csproj ./main/MicrosoftExtensionsHttp/Yardarm.MicrosoftExtensionsHttp.Client/
COPY src/main/NewtonsoftJson/Yardarm.NewtonsoftJson/*.csproj ./main/NewtonsoftJson/Yardarm.NewtonsoftJson/
COPY src/main/NewtonsoftJson/Yardarm.NewtonsoftJson.Client/*.csproj ./main/NewtonsoftJson/Yardarm.NewtonsoftJson.Client/
COPY src/main/NodaTime/Yardarm.NodaTime/*.csproj ./main/NodaTime/Yardarm.NodaTime/
COPY src/main/NodaTime/Yardarm.NodaTime.Client/*.csproj ./main/NodaTime/Yardarm.NodaTime.Client/
COPY src/main/SystemTextJson/Yardarm.SystemTextJson/*.csproj ./main/SystemTextJson/Yardarm.SystemTextJson/
COPY src/main/SystemTextJson/Yardarm.SystemTextJson.Client/*.csproj ./main/SystemTextJson/Yardarm.SystemTextJson.Client/
COPY ["src/*.props", "src/*.targets", "src/*.snk", "src/nuget.config", "./"]
COPY ["src/main/*.props", "src/main/*.targets", "./main/"]
RUN dotnet restore -r $(cat /tmp/arch) -p:PublishReadyToRun=true ./main/Core/Yardarm.CommandLine/Yardarm.CommandLine.csproj

COPY ./src ./
RUN dotnet publish --no-restore -c Release -r $(cat /tmp/arch) -p:VERSION=${VERSION} -o /app ./main/Core/Yardarm.CommandLine/Yardarm.CommandLine.csproj && \
    ln -s /app/Yardarm.CommandLine /app/yardarm

# No --platform here so we get the base image for the target platform
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled
WORKDIR /app
COPY --from=build --chown=$APP_UID:$APP_UID /app/ ./
ENV PATH=/app:${PATH}
ENTRYPOINT ["yardarm"]
