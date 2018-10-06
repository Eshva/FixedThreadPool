#region Usings

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion


namespace Eshva.Threading
{
	public sealed class FixedThreadPool
	{
		public FixedThreadPool(int workCount)
		{
			_workCount = workCount;
			var cancellationToken = _cancellationTokenSource.Token;
			Task.Run(() => ScheduleTaskAsync(cancellationToken), cancellationToken);
		}

		public Task<bool> Execute(ITask task, Priority priority)
		{
			if (_isStopped)
			{
				Console.WriteLine("Tried to execute a task but thread pool already stopped.");
				return Task.FromResult(false);
			}

			switch (priority)
			{
				case Priority.High:
					_highPriorityTasks.Enqueue(task);
					break;
				case Priority.Normal:
					_normalPriorityTasks.Enqueue(task);
					break;
				case Priority.Low:
					_lowPriorityTasks.Enqueue(task);
					break;
				default:
					throw new InvalidEnumArgumentException(nameof(priority), (int)priority, typeof(Priority));
			}

			return Task.FromResult(true);
		}

		public async Task Stop()
		{
			Console.WriteLine("Stop requested.");
			_isStopped = true;
			await WaitAllTaskFinished();
			Console.WriteLine("All tasks finished.");
			_cancellationTokenSource.Cancel();
			Console.WriteLine("Thread pool terminated.");
		}

		private async Task ScheduleTaskAsync(CancellationToken cancellationToken)
		{
			bool AreThereScheduledTasks() => _highPriorityTasks.Any() || _normalPriorityTasks.Any() || _lowPriorityTasks.Any();
			bool IsWorkCapacityReached() => _executingTasks.Count >= _workCount;

			var executedHighTaskCount = 0;
			var currentNormalTaskRatio = NormalTaskRatio1;
			var normalTasksToExecute = 0;

			while (!cancellationToken.IsCancellationRequested)
			{
				LogCurrentState();
				if (AreThereScheduledTasks() &&
					!IsWorkCapacityReached())
				{
					if (executedHighTaskCount >= HighTaskRatio &&
						normalTasksToExecute == 0)
					{
						normalTasksToExecute = currentNormalTaskRatio;
						currentNormalTaskRatio = currentNormalTaskRatio == NormalTaskRatio1 ? NormalTaskRatio2 : NormalTaskRatio1;
					}

					var isExecutionOfNormalTaskAllowed = executedHighTaskCount >= HighTaskRatio && normalTasksToExecute > 0;
					if (_highPriorityTasks.Any())
					{
						var couldHighTaskBeExecuted = !_normalPriorityTasks.Any() || !isExecutionOfNormalTaskAllowed;
						if (couldHighTaskBeExecuted &&
							TryExecuteTaskFromQueue(_highPriorityTasks))
						{
							Console.WriteLine("### High task queued.");
							executedHighTaskCount++;
							continue;
						}
					}

					if (_normalPriorityTasks.Any() &&
						TryExecuteTaskFromQueue(_normalPriorityTasks))
					{
						Console.WriteLine("## Normal task queued.");

						if (normalTasksToExecute > 0)
						{
							normalTasksToExecute--;
						}

						if (normalTasksToExecute == 0)
						{
							executedHighTaskCount = 0;
						}

						continue;
					}

					if (_lowPriorityTasks.Any() &&
						TryExecuteTaskFromQueue(_lowPriorityTasks))
					{
						Console.WriteLine("# Low task queued.");
					}
				}
				else
				{
					Console.WriteLine(
						$"No queued tasks or work capacity is reached. Waiting for {_schedulerTimeout.TotalMilliseconds}ms...");
					await Task.Delay(_schedulerTimeout, cancellationToken);
				}
			}
		}

		private bool TryExecuteTaskFromQueue(ConcurrentQueue<ITask> queue)
		{
			if (queue.TryDequeue(out var task))
			{
				ExecuteAsync(task).ConfigureAwait(false);
				return true;
			}

			return false;
		}

		private async Task ExecuteAsync(ITask task)
		{
			if (_executingTasks.TryAdd(task, new Task(task.Execute)))
			{
				await Task.Run(() => task.Execute());
				_executingTasks.TryRemove(task, out _);
			}
		}

		private async Task WaitAllTaskFinished()
		{
			while (_executingTasks.Any())
			{
				await Task.Delay(_schedulerTimeout);
			}
		}

		private void LogCurrentState() => Console.WriteLine(
			$"Executing {_executingTasks.Count}. Queue: High {_highPriorityTasks.Count}, " +
			$"Normal {_normalPriorityTasks.Count} Low {_lowPriorityTasks.Count}");

		private bool _isStopped;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private readonly ConcurrentDictionary<ITask, Task> _executingTasks = new ConcurrentDictionary<ITask, Task>();
		private readonly ConcurrentQueue<ITask> _highPriorityTasks = new ConcurrentQueue<ITask>();
		private readonly ConcurrentQueue<ITask> _normalPriorityTasks = new ConcurrentQueue<ITask>();
		private readonly ConcurrentQueue<ITask> _lowPriorityTasks = new ConcurrentQueue<ITask>();
		private readonly int _workCount;
		private readonly TimeSpan _schedulerTimeout = TimeSpan.FromMilliseconds(100);
		private const int HighTaskRatio = 5;
		private const int NormalTaskRatio1 = 1;
		private const int NormalTaskRatio2 = 2;
	}
}