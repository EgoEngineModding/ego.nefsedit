// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Progress
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Interface for reporting progress from the library for various operations.
    /// </summary>
    public class NefsProgress
    {
        private static readonly ILogger Log = NefsLib.LogFactory.CreateLogger<NefsProgress>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsProgress"/> class.
        /// </summary>
        public NefsProgress()
            : this(CancellationToken.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsProgress"/> class.
        /// </summary>
        /// <param name="cancelToken">Cancellation token used to cancel the operation.</param>
        public NefsProgress(CancellationToken cancelToken)
        {
            this.CancellationToken = cancelToken;
        }

        /// <summary>
        /// Raised when the progress state changes.
        /// </summary>
        public event EventHandler<NefsProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Cancellation token for the library to monitor.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Total percent complete so far in range [0.0, 1.0].
        /// </summary>
        public float Percent { get; private set; }

        /// <summary>
        /// Current status message.
        /// </summary>
        public string StatusMessage => this.CurrentTask?.Message ?? "";

        /// <summary>
        /// Current status sub-message.
        /// </summary>
        public string StatusSubMessage => this.CurrentTask?.SubMessage ?? "";

        /// <summary>
        /// The current task.
        /// </summary>
        private Task CurrentTask => this.Tasks.Count > 0 ? this.Tasks.Peek() : null;

        /// <summary>
        /// List of currently active tasks. There are no parallel tasks in this list. When a task is
        /// added, it is considered a sub-task of the task before it. The total percent complete is
        /// computed using the weights of each task.
        /// </summary>
        private Stack<Task> Tasks { get; } = new Stack<Task>();

        /// <summary>
        /// Adds a task to the task stack. A sub task is the same as a regular task, except that its
        /// status message is considered a sub-message and it's parent task's status message is preserved.
        /// </summary>
        /// <param name="weight">The weight of this task relative to its parent task.</param>
        /// <param name="subMessage">The status sub-message.</param>
        /// <returns>The new task.</returns>
        public NefsProgressTask BeginSubTask(float weight, string subMessage)
        {
            return this.BeginTask(weight, subMessage, true);
        }

        /// <summary>
        /// Adds a task to the task stack.
        /// </summary>
        /// <param name="weight">
        /// The weight of this task relative to its parent task. The weight is a vlue in the range
        /// of [0.0, 1.0].
        /// </param>
        /// <returns>The new task.</returns>
        public NefsProgressTask BeginTask(float weight)
        {
            return this.BeginTask(weight, null, false);
        }

        /// <summary>
        /// Adds a task to the task stack with a new status message.
        /// </summary>
        /// <param name="weight">
        /// The weight of this task relative to its parent task. The weight is a vlue in the range
        /// of [0.0, 1.0].
        /// </param>
        /// <param name="message">Status message.</param>
        /// <returns>The new task.</returns>
        public NefsProgressTask BeginTask(float weight, string message)
        {
            return this.BeginTask(weight, message, false);
        }

        /// <summary>
        /// Marks the current task as finished. Computes the total percent complete based on the
        /// weight of that task and it's parent tasks, and reports the current progress to the
        /// progress reporter.
        /// </summary>
        public void EndTask()
        {
            // Blow up if a cancellation is requested
            this.CancellationToken.ThrowIfCancellationRequested();

            if (this.Tasks.Count == 0)
            {
                Log.LogDebug("Tried to end a task before starting one.");
                return;
            }

            // Remove current task from stack
            var currentTask = this.Tasks.Pop();

            // Update the parent's remaing percentage
            if (currentTask.Parent != null)
            {
                currentTask.Parent.Remaining -= currentTask.Weight;
            }

            // Update the total progress made
            float progressMade = currentTask.Weight * currentTask.Remaining;

            foreach (var t in this.Tasks)
            {
                progressMade *= t.Weight;
            }

            this.Percent += progressMade;
            this.Percent = Math.Min(this.Percent, 1.0f);
            this.Percent = Math.Max(this.Percent, 0.0f);

            // Notify progress
            this.RaiseProgressChanged();
        }

        private NefsProgressTask BeginTask(float weight, string message, bool isSubTask)
        {
            // Blow up if a cancellation is requested
            this.CancellationToken.ThrowIfCancellationRequested();

            if (weight > 1 || weight < 0)
            {
                throw new ArgumentOutOfRangeException("Task weight must be in range [0.0, 1.0].");
            }

            if (isSubTask && this.Tasks.Count == 0)
            {
                throw new InvalidOperationException("A sub-task cannot be the first task.");
            }

            if (weight == 0)
            {
                Log.LogDebug("Task started with 0 weight.");
            }

            // Create the task
            var parent = this.CurrentTask;
            Task task;

            if (isSubTask)
            {
                // Subtask always uses parent message but sets its own sub-message
                task = new Task(parent, weight, parent.Message, message);
            }
            else
            {
                // Use provided message, otherwise use parent message. No sub-message.
                var msg = message ?? parent?.Message ?? "";
                task = new Task(parent, weight, msg, null);
            }

            // Add a new task to our list
            this.Tasks.Push(task);

            // Report current progress
            this.RaiseProgressChanged();

            return new NefsProgressTask(this);
        }

        private void RaiseProgressChanged()
        {
            var msg = this.StatusMessage;
            var subMsg = this.StatusSubMessage;
            var args = new NefsProgressEventArgs(this.Percent, msg, subMsg);
            this.ProgressChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Used to track task information.
        /// </summary>
        private class Task
        {
            public Task(float weight, string message, string subMessage)
                : this(null, weight, message, subMessage)
            {
            }

            public Task(Task parent, float weight, string message, string subMessage)
            {
                this.Parent = parent;
                this.Weight = weight;
                this.Message = message ?? string.Empty;
                this.SubMessage = subMessage ?? string.Empty;
                this.Remaining = 1.0f;
            }

            /// <summary>
            /// The task status message.
            /// </summary>
            public string Message { get; }

            /// <summary>
            /// The parent task.
            /// </summary>
            public Task Parent { get; }

            /// <summary>
            /// How much of the task's weight is left to complete.
            /// </summary>
            public float Remaining { get; set; }

            /// <summary>
            /// The task sub message.
            /// </summary>
            public string SubMessage { get; }

            /// <summary>
            /// Percentage value of how much weight this task carries. Range of 0.0 to 1.0.
            /// </summary>
            public float Weight { get; }
        }
    }
}
