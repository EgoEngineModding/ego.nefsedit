// See LICENSE.txt for license information.

using System.Text;
using VictorBush.Ego.NefsEdit.Commands;

namespace VictorBush.Ego.NefsEdit.Tests.Commands;

/// <summary>
/// A simple command use for testing the undo buffer.
/// </summary>
internal class TestCommand : INefsEditCommand
{
	public TestCommand(StringBuilder stringBuilder, string oldVal, string newVal)
	{
		TheString = stringBuilder;
		OldVal = oldVal;
		NewVal = newVal;
	}

	private string NewVal { get; }

	private string OldVal { get; }

	private StringBuilder TheString { get; }

	public void Do()
	{
		TheString.Clear();
		TheString.Append(NewVal);
	}

	public void Undo()
	{
		TheString.Clear();
		TheString.Append(OldVal);
	}
}
