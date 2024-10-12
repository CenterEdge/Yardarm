#!/bin/bash

set -e

mkdir -p ./bin
curl -sSL https://api.adv.centeredge.io/v1/swagger/api/swagger.json -o ./bin/mashtub.json

# Basic test of the SDK
dotnet build -c Release src/sdk/Yardarm.Sdk.Test/Yardarm.Sdk.Test.csproj
