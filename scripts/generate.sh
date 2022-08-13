#!/bin/bash

set -e

mkdir -p bin
curl -sSL https://github.com/github/rest-api-description/releases/download/v1.1.4/descriptions.zip -o ./bin/descriptions.zip
unzip -j bin/descriptions.zip descriptions/api.github.com/api.github.com.json -d ./bin/

dotnet run --project src/Yardarm.CommandLine -- generate -n TestSTJ -x src/Yardarm.SystemTextJson/bin/Release/net6.0/Yardarm.SystemTextJson.dll -f netstandard2.0 -f net6.0 --embed -o ./bin -v 1.0.0 -i ./bin/api.github.com.json

dotnet run --project src/Yardarm.CommandLine -- generate -n TestNJ -x src/Yardarm.NewtonsoftJson/bin/Release/net6.0/Yardarm.NewtonsoftJson.dll -f netstandard2.0 -f net6.0 --embed -o ./bin -v 1.0.0 -i ./bin/api.github.com.json
