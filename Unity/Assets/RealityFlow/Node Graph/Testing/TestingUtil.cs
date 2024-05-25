using UnityEngine;

namespace RealityFlow.NodeGraph.Testing
{
    public static class TestingUtil
    {
        /// <summary>
        /// Serializes and then deserializes an object, returning the result.
        /// </summary>
        public static T SerializationRoundTrip<T>(T obj)
        {
            string json = JsonUtility.ToJson(obj);
            return JsonUtility.FromJson<T>(json);
        }
    }
}