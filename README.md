# Yardarm

![Build](https://github.com/CenterEdge/Yardarm/workflows/Build/badge.svg?branch=main&event=push)
[![NuGet](https://img.shields.io/nuget/dt/Yardarm?label=NuGet&logo=NuGet)](https://www.nuget.org/packages/Yardarm)
[![Docker](https://img.shields.io/docker/pulls/centeredge/yardarm?label=Docker&logo=docker)](https://hub.docker.com/repository/docker/centeredge/yardarm)
[![Join the chat at https://gitter.im/CenterEdgeYardarm/community](https://badges.gitter.im/CenterEdgeYardarm/community.svg)](https://gitter.im/CenterEdgeYardarm/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Yardarm is an OpenAPI 3 SDK Generator for C#. It provides various tools that will generate an SDK for a valid OpenAPI specification.

## Features

- Works with valid OpenAPI 3 specs, in JSON or YAML format
- Many generation options
  - Command Line Application
  - Docker Image
  - In-code via library
  - .NET Core Global Tool
  - Support C# 9 Source Generators???
- Generates a SDK using modern C# patterns and practices as follows:
  - Asynchronous methods with cancellation tokens
  - Nullable reference types
  - Interfaces with concrete implementations
    - Should support mocking the implementations for consumers to write unit tests
  - XML documentation for IntelliSense
  - Portable PDB
  - Dependency injection support via `Microsoft.Extensions.DependencyInjection.Abstractions`
- Fast SDK generation
  - Doesn't make C# files on disk and run MSBuild
  - Uses Roslyn to compile directly in memory
- Built-in NuGet support
  - Generate valid `.nupkg` files directly, with correct dependencies
  - Automatically restore packages required for compilation
- Highly extensible SDK generation
  - Built around plugable extensions that add additional features or modify the C# code used for compilation
  - Will include some out-of-the-box plugins for common scenarios
    - `Newtonsoft.Json` serialization/deserialization
    - `NodaTime` date/time properties
- Highly extensible SDK
  - Will allow the SDK consumer to extend the behaviors on the client side
  - Pluggable configuration for things like TLS and base urls
  - Add `HttpMessageHandler` instances to the `HttpClient` stack

## Using Yardarm

### .NET Core Global Tool

```sh
dotnet tool install --global Yardarm.CommandLine --version 0.1.0-alpha003

yardarm help generate
```

### Docker

```sh
docker run -it centeredge/yardarm:release-0.1.0-alpha003

yardarm help generate
```

### Example Command

```sh
# Generate a DLL and related files, with Newtonsoft.Json support
yardarm generate -i my-spec.yaml -n MySpec -v 1.0.0 -o MySpec.dll --xml MySpec.xml --pdb MySpec.pdb -x Yardarm.NewtonsoftJson.dll

# Generate a NuGet package and symbols package, with Newtonsoft.Json support
yardarm generate -i my-spec.json -n MySpec -v 1.0.0 -nupkg MySpec.nupkg --snupkg MySpec.snupkg -x Yardarm.NewtonsoftJson.dll
```

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Authors

* **Brant Burnett** - *Initial work* - [BrantBurnett](https://github.com/brantburnett)

See also the list of [contributors](https://github.com/CenterEdge/Yardarm/graphs/contributors) who participated in this project.

## License

This project is licensed under the Apache 2.0 License - see the [LICENSE.md](LICENSE.md) file for details
