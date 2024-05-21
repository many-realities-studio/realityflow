using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.Collections
{
    /// <summary>
    /// A simple wrapper over a Dictionary<Key, List<Value>> which is also serializable.
    /// </summary>
    [Serializable]
    public class MultiValueDictionary<TKey, TValue>
    {
        [SerializeField]
        SerializableDict<TKey, List<TValue>> dict = new();

        List<TValue> GetOrCreateList(TKey key)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, new());
            return dict[key];
        }

        public void Add(TKey key, TValue value)
        {
            List<TValue> list = GetOrCreateList(key);
            list.Add(value);
        }

        public List<TValue> this[TKey key] => GetOrCreateList(key);
    }
}