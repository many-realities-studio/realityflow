using System.Collections.Generic;

namespace RealityFlow.Collections
{
    public class MultiValueDictionary<TKey, TValue>
    {
        readonly Dictionary<TKey, List<TValue>> dict = new();

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