// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// TryCopyAndReplace is a variant based on String.Replace in .NET.

using System;
using System.Buffers;
using System.Diagnostics;

namespace Yardarm.Internal;

internal static class YardarmMemoryExtensions
{
    /// <param name="source">Source buffer to search and copy.</param>
    extension<T>(ReadOnlySpan<T> source)
        where T : IEquatable<T>?
    {
        /// <summary>
        /// Copies the contents of the source to a new buffer, replacing all occurrences of oldValue with newValue,
        /// if there is at least one instance of oldValue found.
        /// </summary>
        /// <typeparam name="T">Type of element.</typeparam>
        /// <param name="oldValue">Value to replace in the source buffer.</param>
        /// <param name="newValue">Value to place in locations where <paramref name="oldValue"/> is found.</param>
        /// <param name="result">Buffer backed by the <see cref="ArrayPool{T}"/> if replacements were made.</param>
        /// <returns>True if a copy with replacements was placed in <paramref name="result"/>.</returns>
        public bool TryCopyWithReplace(ReadOnlySpan<T> oldValue,
            ReadOnlySpan<T> newValue, out ArrayPoolBuffer<T> result)
        {
            var replacementIndices = new ValueListBuilder<int>(stackalloc int[32]);

            // Find all occurrences of the oldValue
            int i = 0;
            while (true)
            {
                int pos = source[i..].IndexOf(oldValue);
                if (pos < 0)
                {
                    break;
                }

                replacementIndices.Append(i + pos);
                i += pos + oldValue.Length;
            }

            if (replacementIndices.Length == 0)
            {
                result = default;
                return false;
            }

            int length = source.Length + (newValue.Length - oldValue.Length) * replacementIndices.Length;
            var dest = ArrayPool<T>.Shared.Rent(length);

            Span<T> dstSpan = dest.AsSpan(0, length);

            int thisIdx = 0;
            int dstIdx = 0;

            for (int r = 0; r < replacementIndices.Length; r++)
            {
                int replacementIdx = replacementIndices[r];

                // Copy over the non-matching portion of the original that precedes this occurrence of oldValue.
                int count = replacementIdx - thisIdx;
                if (count != 0)
                {
                    source.Slice(thisIdx, count).CopyTo(dstSpan[dstIdx..]);
                    dstIdx += count;
                }
                thisIdx = replacementIdx + oldValue.Length;

                // Copy over newValue to replace the oldValue.
                newValue.CopyTo(dstSpan[dstIdx..]);
                dstIdx += newValue.Length;
            }

            // Copy over the final non-matching portion at the end
            Debug.Assert(source.Length - thisIdx == dstSpan.Length - dstIdx);
            source[thisIdx..].CopyTo(dstSpan[dstIdx..]);

            replacementIndices.Dispose();

            result = new ArrayPoolBuffer<T>(dest, length);
            return true;
        }
    }
}
