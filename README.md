# Yardarm

OpenAPI 3 SDK Generator for C#

## Project Goals

- Works with most valid OpenAPI 3 specs, in JSON or YAML format
- Generated SDK uses modern C# patterns and practices
  - Asynchronous methods with cancellation tokens
  - Nullable reference types
  - Interfaces with concrete implementations
    - Should support mocking the implementations for consumers to write unit tests
  - XML documentation for IntelliSense
  - Portable PDB
  - Targets .NET Standard 2.0 for compatibility
  - Dependency injection support via `Microsoft.Extensions.DependencyInjection.Abstractions`
- Fast SDK generation
  - Doesn't make C# files on disk and run MSBuild
  - Uses Roslyn to compile directly in memory
- Multiple generation options
  - In-code via library
  - Command-line application
  - .NET Core Global Tool
  - Docker image
  - Eventually support C# 9 Source Generators?
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
