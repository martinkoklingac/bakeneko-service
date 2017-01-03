﻿using System;

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
    }
}
