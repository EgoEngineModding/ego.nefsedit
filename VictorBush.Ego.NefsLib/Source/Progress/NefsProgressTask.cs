// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Progress;

/// <summary>
/// Represents a progress task. Exists as a convenience to call EndTask on dispose.
/// </summary>
/// <example>
/// <code>
///using (var t = p.BeginTask(1.0f))
///{
///doSomething();
///}
/// </code>
/// </example>
public sealed class NefsProgressTask : IDisposable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsProgressTask"/> class.
	/// </summary>
	/// <param name="p">The progress context.</param>
	internal NefsProgressTask(NefsProgress p)
	{
		Context = p;
	}

	/// <summary>
	/// Get progress context this task is for.
	/// </summary>
	private NefsProgress Context { get; }

	/// <inheritdoc/>
	public void Dispose()
	{
		Context.EndTask();
	}
}
