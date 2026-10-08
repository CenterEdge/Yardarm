#if !NET5_0_OR_GREATER

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json;

/// <summary>
/// A read-only stream which transcodes the bytes of an inner stream from another encoding to UTF-8 as they are read.
/// </summary>
/// <remarks>
/// <see cref="Encoding.CreateTranscodingStream"/> is not available before .NET 5.
/// </remarks>
internal sealed class TranscodingReadStream : Stream
{
    private const int InputBufferSize = 4096;

    private readonly Stream _inner;
    private readonly Decoder _decoder;
    private readonly Encoder _encoder = new UTF8Encoding(false).GetEncoder();
    private readonly byte[] _input = new byte[InputBufferSize];
    // A decoder may emit one more char than the number of bytes read if it was holding bytes from a previous read
    private readonly char[] _chars = new char[InputBufferSize + 4];
    private readonly byte[] _output;
    private int _outputPosition;
    private int _outputLength;
    private bool _finished;

    public TranscodingReadStream(Stream inner, Encoding innerEncoding)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(innerEncoding);

        _inner = inner;
        _decoder = innerEncoding.GetDecoder();
        _output = new byte[Encoding.UTF8.GetMaxByteCount(_chars.Length)];
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateBuffer(buffer, offset, count);

        while (_outputPosition == _outputLength)
        {
            if (_finished)
            {
                return 0;
            }

            Transcode(_inner.Read(_input, 0, _input.Length));
        }

        return CopyOutput(buffer, offset, count);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ValidateBuffer(buffer, offset, count);

        while (_outputPosition == _outputLength)
        {
            if (_finished)
            {
                return 0;
            }

            Transcode(await _inner.ReadAsync(_input, 0, _input.Length, cancellationToken).ConfigureAwait(false));
        }

        return CopyOutput(buffer, offset, count);
    }

    private void Transcode(int bytesRead)
    {
        // Zero bytes is the end of the stream, flush anything remaining in the decoder and encoder
        bool flush = bytesRead == 0;

        int charCount = _decoder.GetChars(_input, 0, bytesRead, _chars, 0, flush);
        _outputLength = _encoder.GetBytes(_chars, 0, charCount, _output, 0, flush);
        _outputPosition = 0;
        _finished = flush;
    }

    private int CopyOutput(byte[] buffer, int offset, int count)
    {
        int length = Math.Min(count, _outputLength - _outputPosition);
        Buffer.BlockCopy(_output, _outputPosition, buffer, offset, length);
        _outputPosition += length;
        return length;
    }

    private static void ValidateBuffer(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (offset < 0 || count < 0 || buffer.Length - offset < count)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
        }

        base.Dispose(disposing);
    }
}

#endif
