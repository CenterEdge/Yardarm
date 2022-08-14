#!/bin/bash

set -e

mkdir -p ./bin
curl -sSL https://api.adv.centeredge.io/v1/swagger/api/swagger.json -o ./bin/mashtub.json

dotnet run --no-build --no-launch-profile -c Release --project src/Yardarm.CommandLine -- \
    generate -n TestSTJ -x src/Yardarm.SystemTextJson/bin/Release/net6.0/Yardarm.SystemTextJson.dll -f netstandard2.0 net6.0 --embed --nupkg ./bin/ -v 1.0.0 -i ./bin/mashtub.json

dotnet run --no-build --no-launch-profile -c Release --project src/Yardarm.CommandLine -- \
    generate -n TestNJ -x src/Yardarm.NewtonsoftJson/bin/Release/net6.0/Yardarm.NewtonsoftJson.dll -f netstandard2.0 net6.0 --embed --nupkg ./bin/ -v 1.0.0 -i ./bin/mashtub.json
