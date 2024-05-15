using System;
using UnityEngine;

namespace RealityFlow.Collections
{
    [Serializable]
    public struct SerRef<T>
    {
        [SerializeReference]
        public T Value;

        public SerRef(T value) => Value = value;

        public static implicit operator T(SerRef<T> serRef) => serRef.Value;
        public static implicit operator SerRef<T>(T value) => new(value);
    }

    [Serializable]
    public class BoxedSerRef<T>
    {
        [SerializeReference]
        public T Value;

        public BoxedSerRef(T value) => Value = value;

        public static implicit operator T(BoxedSerRef<T> serRef) => serRef.Value;
        public static implicit operator BoxedSerRef<T>(T value) => new(value);
    }
}
