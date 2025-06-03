// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;

namespace VictorBush.Ego.NefsLib.Progress;

/// <summary>
/// Interface for reporting progress from the library for various operations.
/// </summary>
public sealed class NefsProgress
{
	private static readonly ILogger Log = NefsLog.GetLogger();

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
		CancellationToken = cancelToken;
	}

	/// <summary>
	/// Raised when the progress state changes.
	/// </summary>
	public event EventHandler<NefsProgressEventArgs>? ProgressChanged;

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
	public string StatusMessage => CurrentTask?.Message ?? "";

	/// <summary>
	/// Current status sub-message.
	/// </summary>
	public string StatusSubMessage => CurrentTask?.SubMessage ?? "";

	/// <summary>
	/// The current task.
	/// </summary>
	private Task? CurrentTask => Tasks.Count > 0 ? Tasks.Peek() : null;

	/// <summary>
	/// List of currently active tasks. There are no parallel tasks in this list. When a task is added, it is considered
	/// a sub-task of the task before it. The total percent complete is computed using the weights of each task.
	/// </summary>
	private Stack<Task> Tasks { get; } = new Stack<Task>();

	/// <summary>
	/// Adds a task to the task stack. A sub task is the same as a regular task, except that its status message is
	/// considered a sub-message and it's parent task's status message is preserved.
	/// </summary>
	/// <param name="weight">The weight of this task relative to its parent task.</param>
	/// <param name="subMessage">The status sub-message.</param>
	/// <returns>The new task.</returns>
	public NefsProgressTask BeginSubTask(float weight, string? subMessage = null)
	{
		return BeginTask(weight, subMessage, true);
	}

	/// <summary>
	/// Adds a task to the task stack.
	/// </summary>
	/// <param name="weight">
	/// The weight of this task relative to its parent task. The weight is a value in the range of [0.0, 1.0].
	/// </param>
	/// <returns>The new task.</returns>
	public NefsProgressTask BeginTask(float weight)
	{
		return BeginTask(weight, null, false);
	}

	/// <summary>
	/// Adds a task to the task stack with a new status message.
	/// </summary>
	/// <param name="weight">
	/// The weight of this task relative to its parent task. The weight is a vlue in the range of [0.0, 1.0].
	/// </param>
	/// <param name="message">Status message.</param>
	/// <returns>The new task.</returns>
	public NefsProgressTask BeginTask(float weight, string message)
	{
		return BeginTask(weight, message, false);
	}

	/// <summary>
	/// Marks the current task as finished. Computes the total percent complete based on the weight of that task and
	/// it's parent tasks, and reports the current progress to the progress reporter.
	/// </summary>
	public void EndTask()
	{
		// Blow up if a cancellation is requested
		CancellationToken.ThrowIfCancellationRequested();

		if (Tasks.Count == 0)
		{
			Log.LogDebug("Tried to end a task before starting one.");
			return;
		}

		// Remove current task from stack
		var currentTask = Tasks.Pop();

		// Update the parent's remaing percentage
		if (currentTask.Parent != null)
		{
			currentTask.Parent.Remaining -= currentTask.Weight;
		}

		// Update the total progress made
		float progressMade = currentTask.Weight * currentTask.Remaining;

		foreach (var t in Tasks)
		{
			progressMade *= t.Weight;
		}

		Percent += progressMade;
		Percent = Math.Min(Percent, 1.0f);
		Percent = Math.Max(Percent, 0.0f);

		// Notify progress
		RaiseProgressChanged();
	}

	private NefsProgressTask BeginTask(float weight, string? message, bool isSubTask)
	{
		// Blow up if a cancellation is requested
		CancellationToken.ThrowIfCancellationRequested();

		if (weight > 1 || weight < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(weight), "Task weight must be in range [0.0, 1.0].");
		}

		if (isSubTask && Tasks.Count == 0)
		{
			throw new InvalidOperationException("A sub-task cannot be the first task.");
		}

		if (weight == 0)
		{
			Log.LogDebug("Task started with 0 weight.");
		}

		// Create the task
		var parent = CurrentTask;
		Task task;

		if (isSubTask)
		{
			// Subtask always uses parent message but sets its own sub-message
			task = new Task(parent, weight, parent?.Message, message);
		}
		else
		{
			// Use provided message, otherwise use parent message. No sub-message.
			var msg = message ?? parent?.Message ?? "";
			task = new Task(parent, weight, msg, null);
		}

		// Add a new task to our list
		Tasks.Push(task);

		// Report current progress
		RaiseProgressChanged();

		return new NefsProgressTask(this);
	}

	private void RaiseProgressChanged()
	{
		var msg = StatusMessage;
		var subMsg = StatusSubMessage;
		var args = new NefsProgressEventArgs(Percent, msg, subMsg);
		ProgressChanged?.Invoke(this, args);
	}

	/// <summary>
	/// Used to track task information.
	/// </summary>
	private class Task
	{
		public Task(float weight, string? message, string? subMessage)
			: this(null, weight, message, subMessage)
		{
		}

		public Task(Task? parent, float weight, string? message, string? subMessage)
		{
			Parent = parent;
			Weight = weight;
			Message = message ?? string.Empty;
			SubMessage = subMessage ?? string.Empty;
			Remaining = 1.0f;
		}

		/// <summary>
		/// The task status message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// The parent task.
		/// </summary>
		public Task? Parent { get; }

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
