# Generating Yardarm SDKs

## MSBuild SDK

Create the below as a `.csproj` file and include a `.yaml` or `.json` OpenAPI specification
file in the same directory.

```xml
<Project Sdk="Yardarm.Sdk/0.3.0-beta0001">

  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net6.0</TargetFrameworks>
  </PropertyGroup>

</Project>
```

See [Yardarm SDK](./yardarm-sdk.md) for more information and options.

## Command Line

There are a variety of tools available for generating SDKs from the command line.

### .NET Core Global Tool

```sh
dotnet tool install --global Yardarm.CommandLine --version 0.3.0-beta0001

yardarm help generate
```

### Docker

```sh
docker run -it ghcr.io/centeredge/yardarm:0.3.0-beta0001

yardarm help generate
```

### Example Commands

```sh
# Generate a DLL and related files, with Newtonsoft.Json and DI support
yardarm generate -i my-spec.yaml -n MySpec -v 1.0.0 -o output/directory/ -x Yardarm.NewtonsoftJson Yardarm.MicrosoftExtensionsHttp

# Generate a NuGet package and symbols package, with Newtonsoft.Json and DI support
yardarm generate -i my-spec.json -n MySpec -v 1.0.0 --nupkg output/directory/ -x Yardarm.NewtonsoftJson Yardarm.MicrosoftExtensionsHttp

# Generate a DLL and related files, with System.Text.Json and DI support targeting net6.0
yardarm generate -i my-spec.yaml -n MySpec -v 1.0.0 -f net6.0 -o output/directory/ -x Yardarm.SystemTextJson Yardarm.MicrosoftExtensionsHttp

# Generate a NuGet package and symbols package, with System.Text.Json and DI support targeting net6.0 and netstandard2.0
yardarm generate -i my-spec.json -n MySpec -v 1.0.0 -f net6.0 netstandard2.0 --nupkg output/directory/ -x Yardarm.SystemTextJson Yardarm.MicrosoftExtensionsHttp

# Generate a NuGet package and symbols package, with Newtonsoft.Json, DI support targeting net6.0 and author name and description
yardarm generate -i my-spec.json -n MySpec -v 1.0.0 -f net6.0 --author "Some Name Here" --description "Some description here" --nupkg output/directory/ -x Yardarm.NewtonsoftJson Yardarm.MicrosoftExtensionsHttp

# Note the trailing slash on the output directory. If there is no trailing slash, it is assumed to be a DLL or nupkg file name.
# Related files will be output beside that file.
```

## Source Code

Source code for the generated SDKs is created dynamically, in-memory and is never persisted
to disk. This is an intentional decision for performance. However, the source code itself
may be useful. Perhaps a decompiler would like to offer a better-formatted look at your
generated SDK. Or perhaps the SDK consumer would like to step through the SDK code in their
debugger.

This is supported using the `--embed` command line switch. When this switch is set, the generated
code will be formatted with standard whitespace and embedded in the PDB file with any symbols
output. If generating a NuGet package, they will be included in the `snupkg` file. This makes
the source code available to modern debuggers and decompilers.

> :info: To debug into a generated SDK in Visual Studio, be sure to uncheck Just My Code in the
> debugging options of Visual Studio.

## Reference assemblies

To improve build speed of SDK consumers, a reference assembly is created when:

1. `--ref file.dll` is passed to specify a specific location for a reference assembly.
2. `-o path/` is used to output to a directory. A `ref` subdirectory will be created for the reference assembly.
3. `--nupkg` is  used to create a NuGet package. A reference assembly will be embedded in the package for each target framework.

See [Reference Assemblies](https://docs.microsoft.com/en-us/dotnet/standard/assembly/reference-assemblies) for more
information.

## A note on System.Text.Json support

When using System.Text.Json, it is recommended that you target net6.0 at a minimum. Multi-targeting and including
netstandard2.0 in your NuGet package is fine, but standalone targeting of netstandard2.0 or netcoreapp3.1 have problems.

This is because there are some binary breaking changes between the netstandard2.0 and net6.0 of System.Text.Json.dll
published by Microsoft. The net6.0 version includes public init-only properties, while in netstandard2.0 they have
traditional setters. This means that a Yardarm SDK built against netstandard2.0 may fail if it is ever consumed by a
net6.0 application. Ensuring that you include a net6.0 target avoids this problem.
