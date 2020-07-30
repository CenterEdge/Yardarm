using System;

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
    }
}
