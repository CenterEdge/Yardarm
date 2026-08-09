using System;
using System.IO;
using Microsoft.CodeAnalysis.Text;

namespace Yardarm;

/// <summary>
/// An external file being included in the C# compilation.
/// </summary>
public abstract class IncludedFile
{
    /// <summary>
    /// Path to the source file on disk, if applicable.
    /// </summary>
    public virtual string? FilePath => null;

    /// <summary>
    /// Relative path to use when embedding the source in the debug symbols.
    /// </summary>
    public abstract string SourceEmbeddingPath { get; }

    /// <summary>
    /// Tests whether the file should be included in the generation process.
    /// </summary>
    /// <param name="generationContext">Current generation context.</param>
    /// <returns><see langword="true"/> if the file should be included.</returns>
    public virtual bool ShouldInclude(GenerationContext generationContext) => true;

    /// <summary>
    /// Returns the source text of the included file.
    /// </summary>
    /// <returns>The source text of the included file.</returns>
    public abstract SourceText GetSourceText();

    /// <summary>
    /// Creates a new <see cref="IncludedFile"/> from a file on disk.
    /// </summary>
    /// <param name="filePath">Path to the file on disk.</param>
    /// <param name="basePath">Base path to use for relative paths and for computing the path for embedding source with the debug symbols.</param>
    /// <returns>The new <see cref="IncludedFile"/> object.</returns>
    public static IncludedFile CreateFromFile(string filePath, string? basePath = null) =>
        new IncludedFileFromPath(filePath, basePath);

    private sealed class IncludedFileFromPath : IncludedFile
    {
        private readonly string _filePath;
        private readonly string _sourceEmbeddingPath;

        public override string? FilePath => _filePath;

        public override string SourceEmbeddingPath => _sourceEmbeddingPath;

        public IncludedFileFromPath(string filePath, string? basePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            if (!string.IsNullOrEmpty(basePath))
            {
                // Normalize base path
                basePath = Path.GetFullPath(basePath);
            }

            if (!Path.IsPathRooted(filePath))
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    throw new ArgumentException("File path must be absolute if base path is not provided.", nameof(filePath));
                }

                // Normalize the file path combined with the base path
                filePath = Path.GetFullPath(Path.Join(basePath, filePath));
            }

            if (!string.IsNullOrEmpty(basePath) && filePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                // The file is within the base path; compute the relative path
                _sourceEmbeddingPath = filePath.AsSpan()[basePath.Length..]
                    .TrimStart([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar])
                    .ToString();
            }
            else
            {
                // The file is outside the base path; use the file name only
                _sourceEmbeddingPath = Path.GetFileName(filePath);
            }

            _filePath = filePath;
        }

        public override SourceText GetSourceText()
        {
            using FileStream fileStream = File.OpenRead(_filePath);

            return SourceText.From(fileStream, canBeEmbedded: true);
        }
    }
}
