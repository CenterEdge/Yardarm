using System;
using System.IO;
using System.Runtime.Versioning;
using NuGet.Packaging;

namespace Yardarm.Packaging
{
    public class StreamPackageFile : IPackageFile
    {
        private readonly Stream _stream;

        public StreamPackageFile(Stream stream, string path)
        {
            _stream = stream;

            Path = path;

            TargetFramework = FrameworkNameUtility.ParseFrameworkNameFromFilePath(path, out var effectivePath);
            EffectivePath = effectivePath;

            LastWriteTime = DateTimeOffset.Now;
        }

        public string Path { get; }
        public string EffectivePath { get; }
        public FrameworkName TargetFramework { get; }
        public DateTimeOffset LastWriteTime { get; }

        public Stream GetStream() => _stream;
    }
}
