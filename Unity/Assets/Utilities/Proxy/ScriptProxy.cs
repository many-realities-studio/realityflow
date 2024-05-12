using System;
using System.IO;
using System.Reflection;

namespace RealityFlow.Utilities.Proxy
{
    public class ScriptProxy : MarshalByRefObject
    {
        public F LoadDelegateFromAppDomain<F>(MemoryStream stream)
        where
            F : Delegate
        {
            Assembly assembly = Assembly.Load(stream.ToArray());
            return (F)Delegate.CreateDelegate(
                typeof(F),
                assembly.GetType("Script").GetMethods()[0]
            );
        }
    }
}