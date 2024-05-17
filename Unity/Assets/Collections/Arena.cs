using System;
using System.Collections;
using System.Collections.Generic;
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
        public struct Index
        {
            [SerializeField]
            int value;

            public Index(int index)
            {
                value = index;
            }

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