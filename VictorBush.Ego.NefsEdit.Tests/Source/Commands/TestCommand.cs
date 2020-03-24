// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Tests.Commands
{
    using System.Text;
    using VictorBush.Ego.NefsEdit.Commands;

    /// <summary>
    /// A simple command use for testing the undo buffer.
    /// </summary>
    internal class TestCommand : INefsEditCommand
    {
        public TestCommand(StringBuilder stringBuilder, string oldVal, string newVal)
        {
            this.TheString = stringBuilder;
            this.OldVal = oldVal;
            this.NewVal = newVal;
        }

        private string NewVal { get; }

        private string OldVal { get; }

        private StringBuilder TheString { get; }

        public void Do()
        {
            this.TheString.Clear();
            this.TheString.Append(this.NewVal);
        }

        public void Undo()
        {
            this.TheString.Clear();
            this.TheString.Append(this.OldVal);
        }
    }
}
