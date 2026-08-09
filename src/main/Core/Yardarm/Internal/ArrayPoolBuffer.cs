using System;
using System.Buffers;

namespace Yardarm.Internal
{
    internal readonly ref struct ArrayPoolBuffer<T>
    {
        public T[]? Buffer { get; }
        public int Length { get; }

        public Span<T> Span => Buffer.AsSpan(0, Length);

        public ArrayPoolBuffer(T[] buffer, int length)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentOutOfRangeException.ThrowIfLessThan(length, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(length, buffer.Length);

            Buffer = buffer;
            Length = length;
        }

        public void Dispose()
        {
            if (Buffer is not null) // Maybe null if default struct
            {
                ArrayPool<T>.Shared.Return(Buffer);
            }
        }
    }
}
