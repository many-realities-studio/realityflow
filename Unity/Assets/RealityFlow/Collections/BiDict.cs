using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Immutable;

namespace RealityFlow.Collections
{
    /// <summary>
    /// Dictionary with reverse storage for fast lookups of keys from values.
    /// Has one-to-many semantics: Looking up values associated with a key will produce a list.
    /// </summary>
    [Serializable]
    public class BiDict<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        [NonSerialized]
        readonly Dictionary<TKey, TValue> forward = new();
        [NonSerialized]
        readonly MultiValueDictionary<TValue, TKey> backward = new();

        public bool TryAdd(TKey key, TValue value)
        {
            if (!forward.TryAdd(key, value))
                return false;
            backward.Add(value, key);
            return true;
        }

        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
                throw new ArgumentException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return forward.TryGetValue(key, out value);
        }

        public bool TryGetKeys(TValue value, out ImmutableList<TKey> keys)
        {
            return backward.TryGetValues(value, out keys);
        }

        public bool ContainsKey(TKey key)
        {
            return forward.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return backward.ContainsKey(value);
        }

        public bool Remove(TKey key)
        {
            if (!forward.ContainsKey(key))
                return false;

            TValue value = forward[key];
            forward.Remove(key);
            backward.Remove(value, key);

            return true;
        }

        [SerializeField]
        List<SerTuple<TKey, TValue>> listDict;

        public void OnBeforeSerialize()
        {
            listDict = this.Select(pair => (pair.Key, pair.Value).Ser()).ToList();
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < listDict.Count; i++)
                TryAdd(listDict[i].Item1, listDict[i].Item2);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return forward.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return forward.GetEnumerator();
        }
    }
}