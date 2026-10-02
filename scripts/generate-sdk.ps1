#!/usr/bin/env pwsh
#Requires -Version 7.4

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

New-Item -ItemType Directory -Force ./bin | Out-Null
Invoke-WebRequest https://api.adv.centeredge.io/v1/swagger/api/swagger.json -OutFile ./bin/mashtub.json

# Basic test of the SDK
dotnet build -c Release src/sdk/Yardarm.Sdk.Test/Yardarm.Sdk.Test.csproj
