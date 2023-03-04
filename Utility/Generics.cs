using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Utility
{
    public static class Generics
    {
        #region IsAnyOf

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2)
            where VALUE_T : IEquatable<VALUE_T>
            => value.Equals(otherValue1) || value.Equals(otherValue2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3)
            where VALUE_T : IEquatable<VALUE_T>
            => value.Equals(otherValue1) || value.Equals(otherValue2) || value.Equals(otherValue3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4)
            where VALUE_T : IEquatable<VALUE_T>
            => value.Equals(otherValue1) || value.Equals(otherValue2) || value.Equals(otherValue3) || value.Equals(otherValue4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4, VALUE_T otherValue5)
            where VALUE_T : IEquatable<VALUE_T>
            => value.Equals(otherValue1) || value.Equals(otherValue2) || value.Equals(otherValue3) || value.Equals(otherValue4) || value.Equals(otherValue5);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4, VALUE_T otherValue5, VALUE_T otherValue6)
            where VALUE_T : IEquatable<VALUE_T>
            => value.Equals(otherValue1) || value.Equals(otherValue2) || value.Equals(otherValue3) || value.Equals(otherValue4) || value.Equals(otherValue5) || value.Equals(otherValue6);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4, VALUE_T otherValue5, VALUE_T otherValue6, VALUE_T otherValue7)
            where VALUE_T : IEquatable<VALUE_T>
            => value.Equals(otherValue1) || value.Equals(otherValue2) || value.Equals(otherValue3) || value.Equals(otherValue4) || value.Equals(otherValue5) || value.Equals(otherValue6) || value.Equals(otherValue7);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4, VALUE_T otherValue5, VALUE_T otherValue6, VALUE_T otherValue7, VALUE_T otherValue8)
            where VALUE_T : IEquatable<VALUE_T>
            => value.Equals(otherValue1) || value.Equals(otherValue2) || value.Equals(otherValue3) || value.Equals(otherValue4) || value.Equals(otherValue5) || value.Equals(otherValue6) || value.Equals(otherValue7) || value.Equals(otherValue8);

        #endregion

        #region IsNoneOf

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2)
            where VALUE_T : IEquatable<VALUE_T>
            => !value.IsAnyOf(otherValue1, otherValue2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3)
            where VALUE_T : IEquatable<VALUE_T>
            => !value.IsAnyOf(otherValue1, otherValue2, otherValue3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4)
            where VALUE_T : IEquatable<VALUE_T>
            => !value.IsAnyOf(otherValue1, otherValue2, otherValue3, otherValue4);

        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4, VALUE_T otherValue5)
            where VALUE_T : IEquatable<VALUE_T>
            => !value.IsAnyOf(otherValue1, otherValue2, otherValue3, otherValue4, otherValue5);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4, VALUE_T otherValue5, VALUE_T otherValue6)
            where VALUE_T : IEquatable<VALUE_T>
            => !value.IsAnyOf(otherValue1, otherValue2, otherValue3, otherValue4, otherValue5, otherValue6);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4, VALUE_T otherValue5, VALUE_T otherValue6, VALUE_T otherValue7)
            where VALUE_T : IEquatable<VALUE_T>
            => !value.IsAnyOf(otherValue1, otherValue2, otherValue3, otherValue4, otherValue5, otherValue6, otherValue7);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4, VALUE_T otherValue5, VALUE_T otherValue6, VALUE_T otherValue7, VALUE_T otherValue8)
            where VALUE_T : IEquatable<VALUE_T>
            => !value.IsAnyOf(otherValue1, otherValue2, otherValue3, otherValue4, otherValue5, otherValue6, otherValue7, otherValue8);

        #endregion

        #region IsInRange

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange<VALUE_T>(this VALUE_T value, VALUE_T minimumValue, VALUE_T maximumValue)
            where VALUE_T : IComparable<VALUE_T>
            => value.CompareTo(minimumValue) >= 0 && value.CompareTo(maximumValue) <= 0;

        #endregion

        #region IsOutOfRange

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOutOfRange<VALUE_T>(this VALUE_T value, VALUE_T minimumValue, VALUE_T maximumValue)
            where VALUE_T : IComparable<VALUE_T>
            => !value.IsInRange(minimumValue, maximumValue);

        #endregion

        #region None

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None<ELEMENT_T>(this IEnumerable<ELEMENT_T> source)
            => !source.Any();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None<ELEMENT_T>(this IEnumerable<ELEMENT_T> source, Func<ELEMENT_T, bool> predicate)
            => !source.Any(predicate);

        #endregion

        #region NotAny

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NotAny<ELEMENT_T>(this IEnumerable<ELEMENT_T> source, Func<ELEMENT_T, bool> predicate)
            => !source.All(predicate);

        #endregion
    }
}
