namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// There exists an odd bug in .net framework where sometimes it just fails to compile complaining
    /// that this specific type doesn't exist. This type definition exists solely to stop that error.
    /// Source here: 
    /// https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
    /// 
    /// I don't know why it occurs or why this is necessary, but the fix (this type definition)
    /// doesn't hurt anything.
    /// </summary>
    internal static class IsExternalInit { }
}