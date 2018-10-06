#region Usings

using System;
using System.Threading;
using System.Threading.Tasks;
using Eshva.Threading;

#endregion


namespace ShowRunner
{
	public sealed class Program
	{
		public static async Task Main()
		{
			var threadPool = new FixedThreadPool(workCount : 9);
			await ExecuteTasks(threadPool, 100);

			await Task.Delay(TimeSpan.FromSeconds(3));
			Console.WriteLine("Add some more tasks.");
			await ExecuteTasks(threadPool, 50);

			await Task.Delay(TimeSpan.FromSeconds(3));
			Console.WriteLine("Stop the thread pool.");
			await threadPool.Stop();

			Console.WriteLine("Simulate executing tasks after the thread pool is stopped.");
			await ExecuteTasks(threadPool, 10);
		}

		private static async Task ExecuteTasks(FixedThreadPool threadPool, int numberOfTasks)
		{
			Console.WriteLine($"{numberOfTasks} tasks will be executed.");
			var random = new Random();
			for (var i = 0; i < numberOfTasks; i++)
			{
				var priority = (Priority)random.Next((int)Priority.High, (int)Priority.Low + 1);
				var taskDelayInMilliseconds = random.Next(10, 1000);
				var task = new ShowTask(taskDelayInMilliseconds);
				await threadPool.Execute(task, priority);
			}
		}

		private sealed class ShowTask : ITask
		{
			public ShowTask(int taskDelayInMilliseconds)
			{
				_taskDelayInMilliseconds = taskDelayInMilliseconds;
			}

			public void Execute()
			{
				Console.WriteLine(
					$"A task with delay {_taskDelayInMilliseconds}ms started. Thread ID: {Thread.CurrentThread.ManagedThreadId}");
				Thread.Sleep(_taskDelayInMilliseconds);
				Console.WriteLine(
					$"A task with delay {_taskDelayInMilliseconds}ms finished. Thread ID: {Thread.CurrentThread.ManagedThreadId}");
			}

			private readonly int _taskDelayInMilliseconds;
		}
	}
}