// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Commands
{
    using System.Collections.Generic;

    /// <summary>
    /// Command buffer that supports undo/redo.
    /// </summary>
    internal class UndoBuffer
    {
        private readonly List<INefsEditCommand> commands = new List<INefsEditCommand>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UndoBuffer"/> class.
        /// </summary>
        public UndoBuffer()
        {
            this.Reset();
        }

        /// <summary>
        /// Gets the current commands list.
        /// </summary>
        public IReadOnlyList<INefsEditCommand> Commands => this.commands;

        /// <summary>
        /// Gets a value indicating whether the undo buffer is in a modified state.
        /// </summary>
        public bool IsModified => this.PreviousCommandIndex != this.SavedCommandIndex;

        /// <summary>
        /// Gets the index of the next command (for redo).
        /// </summary>
        public int NextCommandIndex { get; private set; }

        /// <summary>
        /// Gets the index of the previous command (for undo).
        /// </summary>
        public int PreviousCommandIndex { get; private set; }

        /// <summary>
        /// Gets the index of the command that represents the unmodified state for a file.
        /// </summary>
        public int SavedCommandIndex { get; private set; }

        /// <summary>
        /// Executes a command and adds it to the undo buffer.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void Execute(INefsEditCommand command)
        {
            command.Do();

            // Clear any redo commands (can only undo after executing a new command)
            if (this.NextCommandIndex < this.commands.Count)
            {
                this.commands.RemoveRange(this.NextCommandIndex, this.commands.Count - this.NextCommandIndex);
            }

            // Add command
            this.commands.Add(command);

            // Update command index
            this.PreviousCommandIndex = this.NextCommandIndex;
            this.NextCommandIndex = this.commands.Count;
        }

        /// <summary>
        /// Marks the current undo buffer state as the unmodified state.
        /// </summary>
        public void MarkAsSaved()
        {
            this.SavedCommandIndex = this.NextCommandIndex;
        }

        /// <summary>
        /// Executes the next command in the undo buffer, if available.
        /// </summary>
        public void Redo()
        {
            if (this.NextCommandIndex >= this.commands.Count
                || this.NextCommandIndex < 0)
            {
                return;
            }

            this.commands[this.NextCommandIndex].Do();
            this.NextCommandIndex += 1;
            this.PreviousCommandIndex += 1;
        }

        /// <summary>
        /// Resets the undo buffer.
        /// </summary>
        public void Reset()
        {
            this.commands.Clear();
            this.NextCommandIndex = 0;
            this.PreviousCommandIndex = -1;
            this.SavedCommandIndex = -1;
        }

        /// <summary>
        /// Reverts the previous command in the undo buffer, if available.
        /// </summary>
        public void Undo()
        {
            if (this.PreviousCommandIndex < 0
                || this.PreviousCommandIndex >= this.commands.Count)
            {
                return;
            }

            this.commands[this.PreviousCommandIndex].Undo();
            this.PreviousCommandIndex -= 1;
            this.NextCommandIndex -= 1;
        }
    }
}
