using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace RealityFlow.Collections
{
    /// <summary>
    /// A simple wrapper over a Dictionary<Key, List<Value>> which is also serializable.
    /// </summary>
    [Serializable]
    public class MultiValueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, ImmutableList<TValue>>>
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

        public bool TryGetValues(TKey key, out ImmutableList<TValue> values)
        {
            if (dict.TryGetValue(key, out List<TValue> vals))
            {
                values = vals.ToImmutableList();
                return true;
            }

            values = null;
            return false;
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

        public bool RemoveAll(TKey key) => dict.Remove(key);

        public bool Contains(TKey key, TValue value)
        {
            return dict.TryGetValue(key, out List<TValue> values) && values.Contains(value);
        }

        public bool ContainsKey(TKey key)
            => dict.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, ImmutableList<TValue>>> GetEnumerator()
            => dict
                .Select(kv => new KeyValuePair<TKey, ImmutableList<TValue>>(kv.Key, kv.Value.ToImmutableList()))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public ImmutableList<TValue> this[TKey key] => GetOrCreateList(key).ToImmutableList();
    }
}