using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons.Ext
{
    public static class EnumExt
    {
        #region PUBLIC METHODS
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of all possible values of enum <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Must be an enum type</typeparam>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        /// <exception cref="ArgumentException">Thrown if <typeparamref name="T"/> is not an enum type</exception>
        public static IEnumerable<T> GetValues<T>() 
            where T : struct
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException("Generic argument bust be an enum type", "T");

            return Enum
                .GetValues(type)
                .Cast<T>();
        }
        #endregion
    }
}
