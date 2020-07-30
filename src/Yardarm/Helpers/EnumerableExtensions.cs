using System;
using System.Collections;
using System.Collections.Generic;

namespace Yardarm.Helpers
{
    public static class EnumerableExtensions
    {
        public static (T Item, IEnumerable<T> Remainder) Pop<T>(this IEnumerable<T> source)
        {
            var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The collection is empty.");
            }

            return (enumerator.Current, new EnumeratorWrapper<T>(enumerator));
        }

        private class EnumeratorWrapper<T> : IEnumerable<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public EnumeratorWrapper(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            ~EnumeratorWrapper()
            {
                _enumerator.Dispose();
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (_enumerator.MoveNext())
                {
                    yield return _enumerator.Current;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
