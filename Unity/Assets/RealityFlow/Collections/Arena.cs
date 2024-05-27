using System;
using UnityEngine;

namespace RealityFlow.Collections
{
    /// <summary>
    /// A list with typesafe, generated indices. 
    /// </summary>
    [Serializable]
    public class Arena<T>
    {
        [SerializeField]
        SerializableDict<Index, T> list = new();
        [SerializeField]
        int nextIndex = 0;

        public T this[Index index] => list[index];

        Index Allocate()
        {
            nextIndex += 1;
            return new(nextIndex - 1);
        }

        public Index Add(T value)
        {
            Index index = Allocate();
            list.Add(index, value);
            return index;
        }

        public bool Remove(Index index)
        {
            return list.Remove(index);
        }

        [Serializable]
        public struct Index : IEquatable<Index>
        {
            [SerializeField]
            int value;

            public Index(int index)
            {
                value = index;
            }

            public override bool Equals(object other)
            {
                if (other is Index index)
                    return Equals(index);
                return false;
            }

            public bool Equals(Index other)
            {
                return value == other.value;
            }

            public static bool operator ==(Index lhs, Index rhs) => lhs.Equals(rhs);
            public static bool operator !=(Index lhs, Index rhs) => !(lhs == rhs);

            public override int GetHashCode()
            {
                // source for algorithm: https://stackoverflow.com/questions/664014/what-integer-hash-function-are-good-that-accepts-an-integer-hash-key
                // need to use one because the default int.GetHashCode() impl is identity and that will
                // degrade hashmap performance with incremental indices
                int hash = ((value >> 16) ^ value) * 0x45d9f3b;
                hash = ((hash >> 16) ^ hash) * 0x45d9f3b;
                hash = (hash >> 16) ^ hash;
                return hash;
            }
        }
    }
}