using System;
using System.Collections.Generic;

namespace Yardarm.Helpers
{
    public static class ArrayExtensions
    {
        public static T[] Push<T>(this T[] array, T item)
        {
            var newArray = new T[array.Length + 1];
            newArray[0] = item;

            if (array.Length > 0)
            {
                array.AsSpan().CopyTo(newArray.AsSpan(1));
            }

            return newArray;
        }

        public static T[] Push<T>(this IReadOnlyList<T> list, T item)
        {
            if (list is T[] array)
            {
                Push(array, item);
            }

            var newArray = new T[list.Count + 1];
            newArray[0] = item;

            for (int i = 0; i < list.Count; i++)
            {
                newArray[i + 1] = list[i];
            }

            return newArray;
        }
    }
}
