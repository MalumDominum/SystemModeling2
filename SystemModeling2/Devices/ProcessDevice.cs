using SystemModeling2.Devices.Enums;
using SystemModeling2.Devices.Models;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;

namespace SystemModeling2.Devices;

public sealed class ProcessDevice : Device
{
	#region Logic Properties

	public PriorityQueue<Element, int> Queue { get; }

	public int InQueue => Queue.Count;

	public int MaxQueue { get; init; }

	private DeviceState[] States { get; }

	public List<ProcessDevice>? MigrateOptions { get; set; }

	public List<int>? PrioritizedTypes { get; set; }

	private const int MigrateDiff = 2;

	#endregion

	#region Statistics Properties

	private double[] PreviousTimes { get; set; }

	public List<Element> Processed { get; }

	public int Rejected { get; private set; }

	public int Migrated { get; private set; }

	public double MeanBusyTime { get; private set; }

	public double MeanInQueue { get; private set; } // Must be divided by the modeling time

	public Dictionary<int, double> IncomingTimes { get; } // Must be divided by the modeling time

	#endregion

	#region Constructor

	public ProcessDevice(string name, Func<double> distributionFunc, int maxQueue = int.MaxValue,
		int processorsCount = 1, List<int>? prioritizedTypes = null, StartedConditions? conditions = null)
		: base(name, distributionFunc, processorsCount)
	{
		MaxQueue = maxQueue;
		PrioritizedTypes = prioritizedTypes;
		Queue = new PriorityQueue<Element, int>();
		Processed = new List<Element>();
		IncomingTimes = new Dictionary<int, double>();

		Array.Fill(NextTimes, double.MaxValue);

		States = new DeviceState[processorsCount];
		Array.Fill(States, DeviceState.Free);

		for (var i = 0; i < processorsCount && conditions?.BusyCount != null && i < conditions.BusyCount; i++)
			States[i] = DeviceState.Busy;

		if (conditions?.InQueue == null) return;
		foreach (var elementType in conditions.InQueue)
			PrioritizedEnqueue(new(elementType, 0));
		for (var i = 0; i < InQueue && i < processorsCount; i++)
			NextTimes[i] = distributionFunc.Invoke();
	}

	#endregion

	public void InAction(double currentTime, Element element)
	{
		IncomingStatistics(element, currentTime);
		if (InQueue >= MaxQueue)
		{
			Rejected++;
			ColoredConsole.WriteLine($"Rejected {this}", ConsoleColor.DarkRed);
			return;
		}
		var freeIndex = Array.IndexOf(States, DeviceState.Free);
		if (freeIndex != -1)
		{
			States[freeIndex] = DeviceState.Busy;
			NextTimes[freeIndex] = currentTime + DistributionFunc.Invoke();
			PrioritizedEnqueue(element);
		}
		else
		{
			PrioritizedEnqueue(element);
			ColoredConsole.WriteLine($"In Queue {this}", ConsoleColor.DarkYellow);
		}
	}

	public override void OutAction(double currentTime)
	{
		var element = Queue.Dequeue();
		var processorI = Array.IndexOf(NextTimes, currentTime);
		FinishedBy[processorI]++;
		Processed.Add(element);

		var nextDevice = GetNextDevice(element.Type);
		if (nextDevice != null)
		{
			ColoredConsole.WriteLine($"Pass from {Name} to {nextDevice}", ConsoleColor.DarkGray);
			nextDevice.InAction(currentTime, element);
		}
		else element.OutOfSystemTime = currentTime;

		NextTimes[processorI] = double.MaxValue;
		if (InQueue > 0)
		{
			NextTimes[processorI] = currentTime + DistributionFunc.Invoke();
			MeanBusyTime += currentTime - PreviousTimes[processorI];
			MeanInQueue += InQueue * (currentTime - PreviousTimes[processorI]);
		}
		else States[processorI] = DeviceState.Free;

		var extraActiveProcessors = InQueue - NextTimes.Count(t => t != double.MaxValue);
		for (var i = extraActiveProcessors; i < 0; i++)
		{
			var freeIndex = Array.IndexOf(NextTimes, NextTimes.Where(t => t != double.MaxValue).Max());
			NextTimes[freeIndex] = double.MaxValue;
			States[freeIndex] = DeviceState.Free;
		}
		PreviousTimes[processorI] = currentTime;
		ColoredConsole.WriteLine($"Processed {this}", ConsoleColor.DarkGreen);
	}

	public void TryMigrate(double currentTime)
	{
		var toDevice = MigrateOptions?.Where(d => InQueue - d.InQueue >= MigrateDiff)
									  .FirstOrDefault(d => d.InQueue == MigrateOptions.Min(d => d.InQueue));
		if (toDevice == null) return;
		ColoredConsole.WriteLine($"Migrated to {toDevice.Name} (InQueue: {toDevice.InQueue}) " +
								 $"from {Name} (InQueue: {InQueue})", ConsoleColor.DarkBlue);
		Migrated++;
		toDevice.InAction(currentTime, Queue.Dequeue());
	}

	private void PrioritizedEnqueue(Element element) => Queue.Enqueue(element, GetElementPriority(element));

	private void IncomingStatistics(Element element, double currentTime)
	{
		var incomingTime = IncomingTimes.ContainsKey(element.Type)
			? IncomingTimes[element.Type] + currentTime - PreviousTime
			: currentTime - PreviousTime;
		IncomingTimes.Remove(element.Type);
		IncomingTimes.Add(element.Type, incomingTime);
	}

	private int GetElementPriority(Element element) =>
		PrioritizedTypes != null && PrioritizedTypes.IndexOf(element.Type) != -1
		  ? PrioritizedTypes.IndexOf(element.Type) : int.MaxValue;

	public override string ToString() => $"{Name}: Next Times - {SC.StringifyList(NextTimes)}; " +
										 $"Processed - {SC.StringifyTypesCount(Processed)}; " +
										 $"In Queue - {SC.StringifyTypesCount(Queue.UnorderedItems.Select(t => t.Element).ToList())}";
}