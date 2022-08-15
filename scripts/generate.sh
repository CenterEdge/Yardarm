#!/bin/bash

set -e

mkdir -p ./bin
curl -sSL https://api.adv.centeredge.io/v1/swagger/api/swagger.json -o ./bin/mashtub.json

dotnet run --no-build --no-launch-profile -c Release --project src/Yardarm.CommandLine -- \
    restore -n TestSTJ -x src/Yardarm.SystemTextJson/bin/Release/net6.0/Yardarm.SystemTextJson.dll -f netstandard2.0 net6.0 --intermediate ./obj/ -i ./bin/mashtub.json
dotnet run --no-build --no-launch-profile -c Release --project src/Yardarm.CommandLine -- \
    generate --no-restore -n TestSTJ -x src/Yardarm.SystemTextJson/bin/Release/net6.0/Yardarm.SystemTextJson.dll -f netstandard2.0 net6.0 --embed --intermediate ./obj/ --nupkg ./bin/ -v 1.0.0 -i ./bin/mashtub.json

dotnet run --no-build --no-launch-profile -c Release --project src/Yardarm.CommandLine -- \
    restore -n TestNJ -x src/Yardarm.NewtonsoftJson/bin/Release/net6.0/Yardarm.NewtonsoftJson.dll -f netstandard2.0 net6.0 --intermediate ./obj/  -i ./bin/mashtub.json
dotnet run --no-build --no-launch-profile -c Release --project src/Yardarm.CommandLine -- \
    generate --no-restore -n TestNJ -x src/Yardarm.NewtonsoftJson/bin/Release/net6.0/Yardarm.NewtonsoftJson.dll -f netstandard2.0 net6.0 --embed --intermediate ./obj/ --nupkg ./bin/ -v 1.0.0 -i ./bin/mashtub.json
