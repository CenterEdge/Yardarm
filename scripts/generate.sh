#!/bin/bash

set -e

mkdir -p ./bin
curl -sSL https://api.adv.centeredge.io/v1/swagger/api/swagger.json -o ./bin/mashtub.json

httpext=src/artifacts/bin/Yardarm.MicrosoftExtensionsHttp/release/Yardarm.MicrosoftExtensionsHttp.dll

for extension in "Yardarm.SystemTextJson" "Yardarm.NewtonsoftJson"; do
    jsonext=src/artifacts/bin/$extension/release/$extension.dll

    dotnet run --no-build --no-launch-profile -c Release --project src/main/Yardarm.CommandLine -- \
    restore -n TestSTJ -x $jsonext $httpext -f netstandard2.0 net6.0 net8.0 --intermediate-dir ./obj/

    dotnet run --no-build --no-launch-profile -c Release --project src/main/Yardarm.CommandLine -- \
    generate --no-restore -n TestSTJ -x $jsonext $httpext -f netstandard2.0 net6.0 net8.0 --embed --intermediate-dir ./obj/ --nupkg ./bin/ -v 1.0.0 -i ./bin/mashtub.json
done
