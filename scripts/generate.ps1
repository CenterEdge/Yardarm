#!/usr/bin/env pwsh
#Requires -Version 7.4

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

function Invoke-Yardarm {
    dotnet run --no-build --no-launch-profile -c Release --project src/main/Core/Yardarm.CommandLine -- @args
}

New-Item -ItemType Directory -Force ./bin | Out-Null
Invoke-WebRequest https://api.adv.centeredge.io/v1/swagger/api/swagger.json -OutFile ./bin/mashtub.json

$httpext = 'src/artifacts/bin/Yardarm.MicrosoftExtensionsHttp/release/Yardarm.MicrosoftExtensionsHttp.dll'
$frameworks = 'netstandard2.0', 'net8.0', 'net9.0', 'net10.0'

foreach ($extension in 'Yardarm.SystemTextJson', 'Yardarm.NewtonsoftJson') {
    $jsonext = "src/artifacts/bin/$extension/release/$extension.dll"

    Invoke-Yardarm restore -n TestSTJ -x $jsonext $httpext -p ExternallyDiscriminatedUnions=true -f $frameworks --intermediate-dir ./obj/

    Invoke-Yardarm generate --no-restore -n TestSTJ -x $jsonext $httpext -f $frameworks --embed --intermediate-dir ./obj/ --nupkg ./bin/ -v 1.0.0 -i ./bin/mashtub.json

    Invoke-Yardarm generate --no-restore -n TestSTJ -x $jsonext $httpext -p ExternallyDiscriminatedUnions=true -f $frameworks --embed --intermediate-dir ./obj/ --nupkg ./bin/ -v 1.0.0 -i ./src/main/Core/Yardarm.CommandLine/swagger.json

    Invoke-Yardarm generate --no-restore -n TestSTJ -x $jsonext $httpext -p ExternallyDiscriminatedUnions=true -f $frameworks --embed --intermediate-dir ./obj/ --nupkg ./bin/ -v 1.0.0 -i ./src/main/Core/Yardarm.CommandLine/swagger.3.1.json
}
