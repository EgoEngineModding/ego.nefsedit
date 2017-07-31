using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib
{
    /// <summary>
    /// Contains data passed the NeFS library during loading and
    /// saving operations to allow progress to be reported.
    /// </summary>
    public class NefsProgressInfo
    {
        private static readonly ILog log = LogHelper.GetLogger();

        /// <summary>The current status message.</summary>
        string _currentMessage;

        /// <summary>Total percent complete so far (0.0 to 1.0).</summary>
        float _totalPercent;

        /// <summary>
        /// List of currently active tasks. There are no parallel tasks in this list.
        /// When a task is added, it is considered a sub-task of the task before it.
        /// The total percent complete is computed using the weights of each task.
        /// </summary>
        List<Task> _tasks = new List<Task>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public NefsProgressInfo()
        { }
        
        /// <summary>
        /// Cancellation token for the library to monitor.
        /// </summary>
        public CancellationToken CancellationToken
        { get; set; }

        /// <summary>
        /// Progress interface to report progress back to.
        /// </summary>
        public IProgress<NefsProgress> Progress
        { get; set; }

        /// <summary>
        /// Adds a task to the task list.
        /// </summary>
        /// <param name="weight">The weight of this task relative to its parent task.</param>
        public void BeginTask(float weight)
        {
            /* Blow up if a cancellation is requested */
            CancellationToken.ThrowIfCancellationRequested();

            if (weight > 1 || weight < 0)
            {
                throw new ArgumentOutOfRangeException("Task weight must be in range [0.0 - 1.0].");
            }

            if (weight == 0)
            {
                log.Debug("Task started with 0 weight.");
            }

            /* Add a new task to our list */
            var task = new Task(weight);
            _tasks.Add(task);
        }

        /// <summary>
        /// Adds a task to the task list and sends a progress report with the specified message.
        /// </summary>
        /// <param name="weight">The weight of this task relative to its parent task.</param>
        /// <param name="message">A status message to report.</param>
        public void BeginTask(float weight, string message)
        {
            BeginTask(weight);

            _currentMessage = message;

            /* A status message was provided, so report it */
            Progress.Report(new NefsProgress()
            {
                Progress = _totalPercent,
                Message = _currentMessage
            });
        }

        /// <summary>
        /// Marks the current task as finished. Computes the total percent complete
        /// based on the weight of that task and it's parent tasks, and reports
        /// the current progress to the progress reporter.
        /// </summary>
        public void EndTask()
        {
            /* Blow up if a cancellation is requested */
            CancellationToken.ThrowIfCancellationRequested();

            if (_tasks.Count == 0)
            {
                log.Debug("Tried to end a task before starting one.");
                return;
            }

            /* Get the current task */
            var currentTask = _tasks[_tasks.Count - 1];

            /* Update the parent's remaing percentage */
            if (_tasks.Count > 1)
            {
                var parentTask = _tasks[_tasks.Count - 2];
                parentTask.Remaining -= currentTask.Weight;
            }

            /* Update the total progress made */
            float progressMade = currentTask.Weight * currentTask.Remaining;

            for (int i = 0; i < _tasks.Count - 1; i++)
            {
                progressMade *= _tasks[i].Weight;
            }

            _totalPercent += progressMade;
            _totalPercent = Math.Min(_totalPercent, 1.0f);
            _totalPercent = Math.Max(_totalPercent, 0.0f);

            Progress.Report(new NefsProgress()
            {
                Progress = _totalPercent,
                Message = _currentMessage
            });

            /* Done with the current task */
            _tasks.Remove(currentTask);
        }

        /// <summary>
        /// Shortcut to report progress to the assigned progress interface without using
        /// the task-based functions.
        /// </summary>
        /// <param name="percent">Percent complete (in range 0.0 to 1.0).</param>
        /// <param name="message">Status message to display.</param>
        public void Report(float percent, string message)
        {
            _totalPercent = percent;
            _currentMessage = message;

            /* Report progress through the assigned progress reporter */
            Progress.Report(new NefsProgress()
            {
                Progress = percent,
                Message = message
            });
        }

        /// <summary>
        /// Used by the ProgressInfo class to track task information.
        /// </summary>
        class Task
        {
            float _weight;

            public Task(float weight)
            {
                _weight = weight;
                Remaining = 1.0f;
            }

            /// <summary>
            /// Percentage value of how much weight this task carries. Range of 0.0 to 1.0.
            /// </summary>
            public float Weight
            {
                get { return _weight; }
            }

            /// <summary>
            /// How much of the task's weight is left to complete.
            /// </summary>
            public float Remaining { get; set; }
        }
    }
}
