using System;

namespace RealityFlow.Collections
{
    /// <summary>
    /// A serializable version of a tuple. Converts back and forth easily.
    /// </summary>
    [Serializable]
    public struct SerTuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public SerTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public readonly (T1, T2) Tup()
            => (Item1, Item2);

        public static implicit operator (T1, T2)(SerTuple<T1, T2> tup) => tup.Tup();
        public static implicit operator SerTuple<T1, T2>((T1, T2) tup) => new(tup.Item1, tup.Item2);
    }

    public static class SerTupleExtensions
    {
        public static SerTuple<T1, T2> Ser<T1, T2>(this (T1, T2) tuple)
            => new(tuple.Item1, tuple.Item2);
    }
}