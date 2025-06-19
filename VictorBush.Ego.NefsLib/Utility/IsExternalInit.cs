// See LICENSE.txt for license information.

using System.ComponentModel;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Enables use of C# record init while still targeting .net standard.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal class IsExternalInit
{ }
