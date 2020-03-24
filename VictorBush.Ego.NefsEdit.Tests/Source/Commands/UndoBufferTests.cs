// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Tests.Commands
{
    using System.Text;
    using VictorBush.Ego.NefsEdit.Commands;
    using Xunit;

    public class UndoBufferTests
    {
        [Fact]
        public void Execute_BufferEmpty_CommandExecuted()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd = new TestCommand(str, "", "new");

            buffer.Execute(cmd);
            Assert.Equal("new", str.ToString());
            Assert.Equal(1, buffer.NextCommandIndex);
            Assert.Equal(0, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);
        }

        [Fact]
        public void Execute_BufferNotEmpty_CommandExecuted()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd1 = new TestCommand(str, "", "A");
            var cmd2 = new TestCommand(str, "A", "B");

            buffer.Execute(cmd1);
            buffer.Execute(cmd2);
            Assert.Equal("B", str.ToString());
            Assert.Equal(2, buffer.NextCommandIndex);
            Assert.Equal(1, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);
        }

        [Fact]
        public void Execute_NextCommandAtBeginning_CommandExecuted()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd1 = new TestCommand(str, "", "A");
            var cmd2 = new TestCommand(str, "A", "B");
            var cmd3 = new TestCommand(str, "B", "C");

            buffer.Execute(cmd1);
            buffer.Execute(cmd2);
            Assert.Equal("B", str.ToString());
            Assert.Equal(2, buffer.NextCommandIndex);
            Assert.Equal(1, buffer.PreviousCommandIndex);

            buffer.Undo();
            Assert.Equal("A", str.ToString());
            Assert.Equal(1, buffer.NextCommandIndex);
            Assert.Equal(0, buffer.PreviousCommandIndex);

            buffer.Undo();
            Assert.Equal("", str.ToString());
            Assert.Equal(0, buffer.NextCommandIndex);
            Assert.Equal(-1, buffer.PreviousCommandIndex);

            buffer.Execute(cmd3);
            Assert.Equal("C", str.ToString());
            Assert.Equal(1, buffer.NextCommandIndex);
            Assert.Equal(0, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);
        }

        [Fact]
        public void Execute_NextCommandInMiddle_CommandExecuted()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd1 = new TestCommand(str, "", "A");
            var cmd2 = new TestCommand(str, "A", "B");
            var cmd3 = new TestCommand(str, "B", "C");
            var cmd4 = new TestCommand(str, "C", "D");

            buffer.Execute(cmd1);
            buffer.Execute(cmd2);
            buffer.Execute(cmd3);

            buffer.Undo();
            Assert.Equal("B", str.ToString());
            Assert.Equal(2, buffer.NextCommandIndex);
            Assert.Equal(1, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);

            buffer.Execute(cmd4);
            Assert.Equal("D", str.ToString());
            Assert.Equal(3, buffer.NextCommandIndex);
            Assert.Equal(2, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);
        }

        [Fact]
        public void Redo_BufferEmpty_NoChange()
        {
            var buffer = new UndoBuffer();
            buffer.Redo();
            Assert.Equal(0, buffer.NextCommandIndex);
            Assert.Equal(-1, buffer.PreviousCommandIndex);
            Assert.False(buffer.IsModified);
        }

        [Fact]
        public void Redo_NextCommandAtBeginning_Redo()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd1 = new TestCommand(str, "", "A");
            var cmd2 = new TestCommand(str, "A", "B");

            buffer.Execute(cmd1);
            buffer.Execute(cmd2);
            buffer.Undo();
            buffer.Undo();
            buffer.Redo();

            Assert.Equal("A", str.ToString());
            Assert.Equal(1, buffer.NextCommandIndex);
            Assert.Equal(0, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);
        }

        [Fact]
        public void Redo_NextCommandAtEnd_NoChange()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd1 = new TestCommand(str, "", "A");
            var cmd2 = new TestCommand(str, "A", "B");

            buffer.Execute(cmd1);
            buffer.Execute(cmd2);
            buffer.Redo();

            Assert.Equal("B", str.ToString());
            Assert.Equal(2, buffer.NextCommandIndex);
            Assert.Equal(1, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);
        }

        [Fact]
        public void Redo_NextCommandInMiddle_Redo()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd1 = new TestCommand(str, "", "A");
            var cmd2 = new TestCommand(str, "A", "B");
            var cmd3 = new TestCommand(str, "A", "B");

            buffer.Execute(cmd1);
            buffer.Execute(cmd2);
            buffer.Execute(cmd1);
            buffer.Undo();
            buffer.Undo();
            buffer.Redo();

            Assert.Equal("B", str.ToString());
            Assert.Equal(2, buffer.NextCommandIndex);
            Assert.Equal(1, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);
        }

        [Fact]
        public void Undo_BufferEmpty_NoChange()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            buffer.Undo();
            Assert.Equal("", str.ToString());
            Assert.Equal(0, buffer.NextCommandIndex);
            Assert.Equal(-1, buffer.PreviousCommandIndex);
            Assert.False(buffer.IsModified);
        }

        [Fact]
        public void Undo_NextCommandAtBeginning_NoChange()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd1 = new TestCommand(str, "", "A");

            buffer.Execute(cmd1);
            buffer.Undo();
            Assert.Equal("", str.ToString());
            Assert.Equal(0, buffer.NextCommandIndex);
            Assert.Equal(-1, buffer.PreviousCommandIndex);
            Assert.False(buffer.IsModified);

            // Try to undo again - should be no change since nothing to undo
            buffer.Undo();
            Assert.Equal("", str.ToString());
            Assert.Equal(0, buffer.NextCommandIndex);
            Assert.Equal(-1, buffer.PreviousCommandIndex);
            Assert.False(buffer.IsModified);
        }

        [Fact]
        public void Undo_NextCommandAtEnd_Undo()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd1 = new TestCommand(str, "", "A");

            buffer.Execute(cmd1);
            Assert.Equal("A", str.ToString());
            Assert.Equal(1, buffer.NextCommandIndex);
            Assert.Equal(0, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);

            buffer.Undo();
            Assert.Equal("", str.ToString());
            Assert.Equal(0, buffer.NextCommandIndex);
            Assert.Equal(-1, buffer.PreviousCommandIndex);
            Assert.False(buffer.IsModified);
        }

        [Fact]
        public void Undo_NextCommandInMiddle_Undo()
        {
            var str = new StringBuilder();
            var buffer = new UndoBuffer();
            var cmd1 = new TestCommand(str, "", "A");
            var cmd2 = new TestCommand(str, "A", "B");
            var cmd3 = new TestCommand(str, "B", "C");

            buffer.Execute(cmd1);
            buffer.Execute(cmd2);
            buffer.Execute(cmd3);

            buffer.Undo();
            buffer.Undo();

            Assert.Equal("A", str.ToString());
            Assert.Equal(1, buffer.NextCommandIndex);
            Assert.Equal(0, buffer.PreviousCommandIndex);
            Assert.True(buffer.IsModified);
        }

        [Fact]
        public void UndoBuffer_DefaultState()
        {
            var buffer = new UndoBuffer();
            Assert.Equal(0, buffer.NextCommandIndex);
            Assert.Equal(-1, buffer.PreviousCommandIndex);
            Assert.False(buffer.IsModified);
        }
    }
}
