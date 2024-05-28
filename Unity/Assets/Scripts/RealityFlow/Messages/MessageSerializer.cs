using Newtonsoft.Json;
using System;

namespace Packages.realityflow_package.Runtime.scripts.Messages
{
    /// <summary>
    /// The purpose of this class is to provide utility functions to allow for easy serializing and
    /// de-serializing of <see cref="BaseMessage"/> objects.
    /// Note that any new message classes created must contain the [DataContract] tag,
    /// and that each property that you want to serialize must be tagged as a [DataMember].
    /// You cannot serialize fields.
    /// </summary>
    public static class MessageSerializer
    {
        public static string SerializeMessage<T>(T messageToSerialize) where T : BaseMessage
        {
            return JsonConvert.SerializeObject(messageToSerialize, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public static dynamic DesearializeObject(string messageToDeserialize, Type type)
        {
            return JsonConvert.DeserializeObject(messageToDeserialize, type);
        }
    }
}