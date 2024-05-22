using System;
using UnityEngine;

namespace RealityFlow.Collections
{
    /// <summary>
    /// Wraps a value to make it serialized by reference. This is useful e.g. in a `List` where you
    /// want each item to be serialized by reference, but can't apply an attribute like that.
    /// </summary>
    [Serializable]
    public struct SerRef<T>
    where T : class
    {
        [SerializeReference]
        public T Value;

        public SerRef(T value) => Value = value;

        public static implicit operator T(SerRef<T> serRef) => serRef.Value;
        public static implicit operator SerRef<T>(T value) => new(value);
    }
}
