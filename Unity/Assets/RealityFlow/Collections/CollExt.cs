using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RealityFlow.Collections
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Look at the last element in the list.
        /// </summary>
        public static T Peek<T>(this List<T> list) => list[^1];

        /// <summary>
        /// Add an element to the end of the list.
        /// </summary>
        public static void Push<T>(this List<T> list, T value) => list.Add(value);

        /// <summary>
        /// Remove and get the last element of the list.
        /// </summary>
        public static T Pop<T>(this List<T> list)
        {
            T value = list.Peek();
            list.RemoveAt(list.Count - 1);
            return value;
        }

        public static bool In(this int value, Range range)
        {
            (Index start, Index end) = (range.Start, range.End);

            if (start.IsFromEnd || end.IsFromEnd)
                throw new ArgumentException("this method does not support indexing from end");

            bool afterStart = start.Equals(Index.Start) || value >= start.Value;
            bool beforeEnd = end.Equals(Index.End) || value < end.Value;

            return afterStart && beforeEnd;
        }

        public static (List<T>, List<U>) UnzipToLists<T, U>(this IEnumerable<(T, U)> enumerable)
        {
            List<T> ts = new();
            List<U> us = new();
            foreach ((T t, U u) in enumerable)
            {
                ts.Add(t);
                us.Add(u);
            }

            return (ts, us);
        }

        public static bool TryGetValueAs<T>(this JObject coll, string key, out T value)
        where T: JToken
        {
            if (coll.TryGetValue(key, out JToken temp) && temp is T tValue)
            {
                value = tValue;
                return true;
            }
            value = default;
            return false;
        }
    }
}