// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Commands
{
    /// <summary>
    /// A command that can be performed by the application.
    /// </summary>
    internal interface INefsEditCommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        void Do();

        /// <summary>
        /// Reverts the command.
        /// </summary>
        void Undo();
    }
}
