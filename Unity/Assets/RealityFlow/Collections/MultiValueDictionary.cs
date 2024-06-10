using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.Collections
{
    /// <summary>
    /// A simple wrapper over a Dictionary<Key, List<Value>> which is also serializable.
    /// </summary>
    [Serializable]
    public class MultiValueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, List<TValue>>>
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

        public bool Remove(TKey key, TValue value)
        {
            if (dict.TryGetValue(key, out List<TValue> values))
            {
                int index;
                if ((index = values.IndexOf(value)) < 0)
                    return false;

                values.RemoveAt(index);
                return true;
            }

            return false;
        }

        public bool Contains(TKey key, TValue value)
        {
            return dict.TryGetValue(key, out List<TValue> values) && values.Contains(value);
        }

        public bool ContainsKey(TKey key)
            => dict.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator()
            => dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => dict.GetEnumerator();

        public List<TValue> this[TKey key] => GetOrCreateList(key);
    }
}