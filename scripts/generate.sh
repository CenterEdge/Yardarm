#!/bin/bash

set -e

mkdir -p ./bin
curl -sSL https://api.adv.centeredge.io/v1/swagger/api/swagger.json -o ./bin/mashtub.json

dotnet run --no-build --no-launch-profile -c Release --project src/main/Yardarm.CommandLine -- \
    restore -n TestSTJ -x src/main/Yardarm.SystemTextJson/bin/Release/net6.0/Yardarm.SystemTextJson.dll -f netstandard2.0 net6.0 net8.0 --intermediate-dir ./obj/
dotnet run --no-build --no-launch-profile -c Release --project src/main/Yardarm.CommandLine -- \
    generate --no-restore -n TestSTJ -x src/main/Yardarm.SystemTextJson/bin/Release/net6.0/Yardarm.SystemTextJson.dll -f netstandard2.0 net6.0 net8.0 --embed --intermediate-dir ./obj/ --nupkg ./bin/ -v 1.0.0 -i ./bin/mashtub.json

dotnet run --no-build --no-launch-profile -c Release --project src/main/Yardarm.CommandLine -- \
    restore -n TestNJ -x src/main/Yardarm.NewtonsoftJson/bin/Release/net6.0/Yardarm.NewtonsoftJson.dll -f netstandard2.0 net6.0 --intermediate-dir ./obj/
dotnet run --no-build --no-launch-profile -c Release --project src/main/Yardarm.CommandLine -- \
    generate --no-restore -n TestNJ -x src/main/Yardarm.NewtonsoftJson/bin/Release/net6.0/Yardarm.NewtonsoftJson.dll -f netstandard2.0 net6.0 --embed --intermediate-dir ./obj/ --nupkg ./bin/ -v 1.0.0 -i ./bin/mashtub.json

# Basic test of the SDK
dotnet build -c Release src/sdk/Yardarm.Sdk.Test/Yardarm.Sdk.Test.csproj
