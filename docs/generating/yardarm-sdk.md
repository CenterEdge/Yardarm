# Yardarm SDK

The Yardarm SDK is designed to easily integrate SDK generation into your MSBuild pipeline.
It provides a familiar `.csproj` style for defining SDK projects which are then built and
managed much like any other C# project.

## Features

- Incremental builds
- Target one or more frameworks
- Extension points for dynamically downloading OpenAPI specifications
- Configure Yardarm extensions
- Automatic NuGet package references
  - Includes support for [Central Package Management](https://devblogs.microsoft.com/nuget/introducing-central-package-management/)
- Include the project in Visual Studio solutions
- Reference the project as a `PackageReference` from other .NET projects
- `dotnet restore`, `dotnet build`, and `dotnet pack` support

## Example Project

Create the below as a `.csproj` file and include a `.yaml` or `.json` OpenAPI specification
file in the same directory.

```xml
<Project Sdk="Yardarm.Sdk/0.3.0-beta0001">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <!--
    Any ".json", ".yaml", or ".yml" file in the same directory is assumed to be the OpenAPI Spec.
    However, if the file is located elsewhere it may be referenced manually.
  -->
  <ItemGroup>
    <OpenApiSpec Include="../my-spec.yaml" />
  </ItemGroup>

</Project>
```

## Multi-targeting

```xml
<Project Sdk="Yardarm.Sdk/0.3.0-beta0001">

  <PropertyGroup>
    <!-- Note that Yardarm doesn't support net4x targets, except via netstandard2.0 -->
    <TargetFrameworks>netstandard2.0;net6.0</TargetFrameworks>
  </PropertyGroup>

</Project>
```

## Yardarm Extensions

```xml
<Project Sdk="Yardarm.Sdk/0.3.0-beta0001">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>

    <JsonMode>NewtonsoftJson</JsonMode> <!-- Use Newtonsoft.Json instead of the default System.Text.Json. Set to "None" to disable JSON support. -->
    <DependencyInjectionMode>None</DependencyInjectionMode> <!-- Disable the default Microsoft.Extensions.Http DI extension -->
  </PropertyGroup>

  <ItemGroup>
    <!-- Add one or more other extensions -->
    <YardarmExtension Include="Path/To/My/Extension.dll" />
  </ItemGroup>

</Project>
```

## Project Properties

A wide variety of C# project properties are supported as well. These may also be passed as parameters to a command-line build,
i.e. `dotnet build /p:Version=5.6.7`.

```xml
<Project Sdk="Yardarm.Sdk/0.3.0-beta0001">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>

    <AssemblyName>SomeOtherAssemblyName</AssemblyName>
    <RootNamespace>Some.Other.Namespace</RootNamespace>
    <Version>5.6.7</Version>
    <EmbedAllSources>false</EmbedAllSources> <!-- Defaults to true -->
    <IncludeSymbols>false</IncludeSymbols> <!-- Defaults to true -->
    <GenerateDocumentationFile>false</GenerateDocumentationFile> <!-- Defaults to true -->
    <ProduceReferenceAssembly>true</ProduceReferenceAssembly> <!-- Default varies by target framework, follows C# defaults -->
    <KeyFile>Key.snk</KeyFile> <!-- Strong naming key -->

    <!-- All standard settings related to NuGet packaging apply -->
    <PackageId>ClassLibDotNetStandard</PackageId>
    <Authors>your_name</Authors>
    <Company>your_company</Company>
    <PackageTags>tag1;tag2</PackageTags>
    <Description>This is a long description</Description>
  </PropertyGroup>

</Project>
```

## Downloading specs dynamically

The OpenAPI spec itself may be coming from an outside source, such as a website
or another project in your build tree. The `CollectOpenApiSpecs` target may be
overridden in MSBuild to dynamically collect spec files via any means available
within MSBuild. When the target is complete, the `OpenApiSpec` item should be
added to the project.

```xml
<Project Sdk="Yardarm.Sdk/0.3.0-beta0001">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <Target Name="CollectOpenApiSpecs">
    <DownloadFile SourceUrl="https://generator.swagger.io/api/swagger.json"
                  DestinationFolder="$(IntermediateOutputPath)"
                  SkipUnchangedFiles="true">
      <Output TaskParameter="DownloadedFile" ItemName="_DownloadedOpenApiSpec" />
    </DownloadFile>

    <ItemGroup>
      <OpenApiSpec Include="@(_DownloadedOpenApiSpec)" />
      <FileWrites Include="@(_DownloadedOpenApiSpec)" />
    </ItemGroup>
  </Target>

</Project>
```
