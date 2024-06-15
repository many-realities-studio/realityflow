using System;

namespace RealityFlow
{
    public static class UtilExt
    {
        public static U CastTo<T, U>(this T value)
            => value is U uValue ? uValue : throw new InvalidCastException();
    }
}