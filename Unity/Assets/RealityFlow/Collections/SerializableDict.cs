using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RealityFlow.Collections
{
    /// <summary>
    /// A simple wrapper around dictionary to make it serializable. Increases memory consumption somewhat.
    /// </summary>
    [Serializable]
    public class SerializableDict<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<SerTuple<TKey, TValue>> listDict;

        public void OnBeforeSerialize()
        {
            listDict = this.Select(pair => (pair.Key, pair.Value).Ser()).ToList();
        }

        public void OnAfterDeserialize()
        {
            if (listDict != null)
                for (int i = 0; i < listDict.Count; i++)
                    TryAdd(listDict[i].Item1, listDict[i].Item2);
        }
    }
}