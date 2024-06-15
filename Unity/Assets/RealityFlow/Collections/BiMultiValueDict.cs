using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;

namespace RealityFlow.Collections
{
    /// <summary>
    /// A simple wrapper over a Dictionary<Key, List<Value>> which is also serializable.
    /// Contains back edges; This forms a many-to-many relationship. 
    /// </summary>
    [Serializable]
    public class BiMultiValueDict<TKey, TValue> : IEnumerable<KeyValuePair<TKey, List<TValue>>>, ISerializationCallbackReceiver
    {
        [SerializeField]
        MultiValueDictionary<TKey, TValue> forward = new();
        [NonSerialized]
        MultiValueDictionary<TValue, TKey> backward = new();

        public void Add(TKey key, TValue value)
        {
            forward.Add(key, value);
            backward.Add(value, key);
        }

        public bool TryGetValues(TKey key, out ImmutableList<TValue> values)
        {
            return forward.TryGetValues(key, out values);
        }

        public bool TryGetKeys(TValue value, out ImmutableList<TKey> keys)
        {
            return backward.TryGetValues(value, out keys);
        }

        public bool Remove(TKey key, TValue value)
        {
            if (!forward.Remove(key, value))
                return false;
            backward.Remove(value, key);
            return true;
        }

        public bool RemoveAll(TKey key)
        {
            if (!ContainsKey(key))
                return false;

            if (forward.TryGetValues(key, out ImmutableList<TValue> values))
                foreach (TValue value in values)
                    backward.Remove(value, key);
            forward.RemoveAll(key);

            return true;
        }

        public bool Contains(TKey key, TValue value)
        {
            return forward.Contains(key, value);
        }

        public bool ContainsKey(TKey key)
            => forward.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator()
            => forward.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => forward.GetEnumerator();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            foreach ((TKey key, List<TValue> values) in forward)
                foreach (TValue value in values)
                    backward.Add(value, key);
        }

        public ImmutableList<TValue> this[TKey key] => forward[key];
    }
}