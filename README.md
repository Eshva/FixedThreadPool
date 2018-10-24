# FixedThreadPool Challenge
## The Quiz

Implement an analogue of Java FixedThreadPool class with the following requirements:
* Use C#.
* The ```FixedThreadPool``` class constructor should receive the number of concurrently served tasks (TPL Task).
* The ```FixedThreadPool``` class should have the following methods: ```async Task<bool> Execute(ITask task, Priority priority)``` and ```void Stop()```.
* The interface ```ITask``` should have a single method ```void Execute()``` which executing with a rendom TPL task.
* The ```Priority``` is an enumeration with three values: HIGH, NORMAL, LOW.
* There are the following rules for task choosing: (1) Per 5 high proprity tasks should be executed 1 normal priority task, next time 2 normal priority tasks, next time 1 and so on. (2) Low priority tasks should not be executed till there are any queued tasks with higher priorities.
* Until the ```Stop``` method called the tasks are queued with the ```Execute``` method which immediately returns ```true``` with no waiting the task is complete. After the ```Stop``` method called the ```Execute``` method immediately returns ```false``` and the task not queued for execution.
* The method ```Stop``` is waiting for all the queued and executing tasks are complete then returns.
* For implementation the Task based asynchonous programming should be used.

Proposed types are:

```
public class FixedThreadPool
{
	public FixedThreadPool(int workCount) {}
	public async Task<bool> Execute(ITask task, Priority priority) {}
	async Task Stop() {}
}

public interface ITask
{
	void Execute();
}

public enum Priority
{
	HIGH,
	NORMAL, 
	LOW
}
```

## My implementation
1. I haven't understood why the ```FixedThreadPool.Execute``` method should be ```async``` because it should return immediately in both cases. There is nothing to wait in my implementation.
2. In the requirements there is nothing about the case when there are normal priority queued task but no any high priority tasks. I've decided that in this case it's better to give a chance for normal priority tasks to be executed.
3. There is no requirements what to do with the scheduler thread after the ```Stop``` method called and all tasks are complete. I've decided that it should be stopped because there is no requirement to implement the ```IDisposable``` interface and the pool object is useless after it stopped (there is no method to start it again).
4. I used for all lists CLR concurrent collections to implemenent lock-free multi-threading.
5. For testing means I've implemented a console application ```ShowRunner```. For the same means I've added some console tracing into the pool code. Of course it should be avoided in production code. It is a .NET Core console application and there is no an EXE-file we used to. To execute the test application you should use the following command line from the solution root folder:
```dotnet run --project ShowRunner```

# По-русски
## Задание
**Сохранены орфография и пунктуация оригинала.**

Требуется реализация класса, аналогичного FixedThreadPool в Java, со следующими требованиями:

* Решение на C#
* В конструктор этого класса должно передаваться максимальное количество одновременно выполняемых заданий (Task TPL), которые будут выполнять задачи.
* Интерфейс класса должен предоставлять методы: async Task<bool > Execute(ITask task, Priority priority) и void Stop()
* Интерфейс ITask должен содержать один метод: void Execute(), который запускается в произвольном задании.
* Тип Priority — это перечисление из трёх приоритетов: HIGH, NORMAL, LOW. При этом действуют такие правила: на пять задачи с приоритетом HIGH выполняется сначала одна задача с приоритетом NORMAL в следующий раз две, затем снова одна и т.п., задачи с приоритетом LOW не выполняются, пока есть хоть одна задача с другим приоритетом.
* До вызова метода Stop() задачи ставятся в очередь на выполнение и метод Execute() сразу же возвращает true, не дожидаясь завершения выполнения задачи; а после вызова Stop() новые задачи не добавляются в очередь на выполнение, и метод Execute() сразу же возвращает false.
* Метод Stop() ожидает завершения всех текущих задач.
* При реализации задачи необходимо использовать Task based Asynchronous programming 

```
public class FixedThreadPool
{
	public FixedThreadPool(int workCount) {}
	public async Task<bool> Execute(ITask task, Priority priority) {}
	async Task Stop() {}
}

public interface ITask
{
	void Execute();
}

public enum Priority
{
	HIGH,
	NORMAL, 
	LOW
}
```

## Реализация

1. Я не понял, зачем делать метод ```FixedThreadPool.Execute``` async-методом, поскольку он должен возвращать управление моментально в обоих случаях, ожидать просто нечего. В любом случае, в моей реализации это не потребовалось.
2. В части черодования задач с высоким и нормальным приоритетом, я исходил из того, что если нет задач с высоким, но есть задачи с нормальным, задачи с нормальным приоритетом выполняются пока есть таковые, поставленные в очередь на выполнение и не добавлена ни одна задача с высоким. Хотя, можно было бы прочитать, что задачи с нормальным приоритетом выполняются строго после 5 выполненных задач с высоким приоритетом. Я предположил, что не нужно заставлять ждать другие задачи, если есть возможность их выполнить.
3. В задании не указывается, что должно происходить с потоком планировщика после остановки пула. Я предположил, что он должен быть возвращён среде выполнения после того, как все задачи завершат своё исполнение.
4. Для всех списков использовал concurrent-коллекции. Так удалось реализовать lock-free многозадачность.
5. Для проверки работоспособности кода добавил в солюшен консольное приложение, а также вставил в код пула трассирующие сообщения. В production-коде, понятное дело, подобных выводов к консоль быть не должно. Для возможности запуска кода на всех основных платформах, консольное приложение реализованно на .NET Core. Для запуска необходимо выполнить следующую команду, находясь в папке солюшина (привычного экзешника нет):
   ```dotnet run --project ShowRunner```
6. Я когда-то давно, аж в 2012 году, решал данную задачу для [Связной Банк](http://www.banki.ru/banks/memory/bank/?id=8464301) и даже написал о моём опыте на [Хабре](https://habr.com/post/145551/), но реализовывал её на асинхронных примитивах, хотя TPL в то время уже основательно зарекомендовала себя. Подозреваю, что они хотели видеть реализацию именно с использованием TPL, но из задания это не следовало.
7. Потратил примерно 6 часов в несколько подходов (дела, дела), включая время на освоение матчасти. Возможно, в каких-то тонкостях я не разобрался, но приложение, судя по трассировке, работает, как ожидается. Например, я не уверен, безопасно ли использовать LINQ-методы при работе с concurrent-коллекциями.
8. Если бы я разрабатывал подобный пул для реального проекта, я бы вынес логику выбора следующей задачи в стратегию, ведь может так случиться, что принцип работы пула должен быть сохранён, а принцип "1 через 5" измениться.
9. Защиту от повторного вызова метода ```Stop``` реализовывать не стал, ибо экземпляром данного класса должен владеть и управлять кто-то один, а сам метод предполагает ожидание завершения остановки по условиям задачи. Я бы ещё добавил вызов данного метода в методе ```Dispose``` (с реализацией соответствующего интерфейса), так как класс ресурсоёмкий. Опять же реализовывать это не стал, так как этого нет в условиях задачи.
10. Продолжительность _schedulerTimeout выбрана соразмерной продолжительности тестовых задач в демострационных целях. В продуктовом коде она должна быть уменьшена.
