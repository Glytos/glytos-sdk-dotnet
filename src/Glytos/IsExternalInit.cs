// Enables `init`-only setters and records on netstandard2.0, where the runtime does
// not ship the marker type. On net5.0+ the built-in type is used instead.
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
#endif
