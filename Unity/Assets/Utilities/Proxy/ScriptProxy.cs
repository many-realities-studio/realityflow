using System;
using System.IO;
using System.Reflection;

namespace RealityFlow.Utilities.Proxy
{
    public class ScriptProxy : MarshalByRefObject
    {
        public void LoadStaticAssembly(string assemblyLocation)
        {
            Assembly.LoadFrom(assemblyLocation);
        }

        public ScriptDelegate<F> LoadDelegateFromAppDomain<F>(MemoryStream stream)
        where
            F : Delegate
        {
            Assembly assembly = Assembly.Load(stream.ToArray());
            return new((F)Delegate.CreateDelegate(
                typeof(F),
                assembly.GetType("Script").GetMethods()[0]
            ));
        }
    }

    public class ScriptDelegate<F> : MarshalByRefObject
    where
        F : Delegate
    {
        public F Delegate;

        public ScriptDelegate(F del)
        {
            Delegate = del;
        }

        public void Invoke()
        {
            (Delegate as Action).Invoke();
        }
    }
}