using System;
using System.IO;
using System.Runtime.Versioning;
using NuGet.Frameworks;
using NuGet.Packaging;

namespace Yardarm.Packaging
{
    public class StreamPackageFile : IPackageFile
    {
        private readonly Stream _stream;

        public StreamPackageFile(Stream stream, string path, string nugetFramework)
            : this(stream, path, NuGetFramework.Parse(nugetFramework))
        {
        }

        public StreamPackageFile(Stream stream, string path, NuGetFramework nugetFramework)
        {
            _stream = stream;

            Path = path;
            NuGetFramework = nugetFramework;
            LastWriteTime = DateTimeOffset.Now;
        }

        public string Path { get; }
        public string EffectivePath => Path;
        public FrameworkName TargetFramework => null;
        public NuGetFramework NuGetFramework { get; }
        public DateTimeOffset LastWriteTime { get; }

        public Stream GetStream() => _stream;
    }
}
