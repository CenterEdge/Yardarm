![Build](https://github.com/CenterEdge/Yardarm/workflows/Build/badge.svg?branch=main&event=push)

# Yardarm

Yardarm is an OpenAPI 3 SDK Generator for C#. It provides various tools that will generate an SDK for a valid OpenAPI specification.

## Features

- Works with valid OpenAPI 3 specs, in JSON or YAML format
- Generates an SDK within the project
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

In order to generate an SDK from an OpenAPI specification, open up the project and change the input OpenAPI spec and then run the project. The SDK will be generated.

## Project Goals

- More generation options
  - Command Line Application
  - Docker Image
  - In-code via library
  - .NET Core Global Tool
  - Support C# 9 Source Generators?

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Authors

* **Brant Burnett** - *Initial work* - [BrantBurnett](https://github.com/brantburnett)

See also the list of [contributors](https://github.com/CenterEdge/Yardarm/graphs/contributors) who participated in this project.

## License

This project is licensed under the Apache License - see the [LICENSE.md](LICENSE.md) file for details
