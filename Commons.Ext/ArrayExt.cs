using System;

namespace Commons.Ext
{
    public static class ArrayExt
    {
        public static T[] Slice<T>(this T[] array, int startIndex, int endIndex)
        {
            if (array == null)
                return null;

            if (array.Length == 0)
                return new T[0];

            if (startIndex > endIndex)
                throw new ArgumentOutOfRangeException("startIndex", startIndex, $"startIndex ({startIndex}) must be smaller or equal to endIndex ({endIndex})");

            if (startIndex >= array.Length)
                throw new ArgumentOutOfRangeException("startIndex", startIndex, $"startIndex ({startIndex}) is greater then last index of array ({array.Length - 1})");

            if (endIndex >= array.Length)
                throw new ArgumentOutOfRangeException("endIndex", endIndex, $"endIndex ({endIndex}) is greater then last index of array ({array.Length - 1})");

            var length = endIndex - startIndex + 1;
            var result = new T[length];
            Array.Copy(array, startIndex, result, 0, length);

            return result;
        }

        /// <summary>
        /// Check if an <paramref name="array"/> is null or has no elements
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="array"/> elements</typeparam>
        /// <param name="array">array of type <typeparamref name="T"/></param>
        /// <returns>bool</returns>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }
    }
}
