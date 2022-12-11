using System.Xml.Linq;
using SystemModeling2.Devices.Enums;
using SystemModeling2.Devices.Models;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;

namespace SystemModeling2.Devices;

public sealed class ProcessDevice : Device
{
	#region Logic Properties

	public PriorityQueue<Element, int> Queue { get; }

	public int BusyProcessors => States.Count(t => t == DeviceState.Busy);

	public int InQueue => Queue.Count - BusyProcessors;

	public int MaxQueue { get; init; }

	private DeviceState[] States { get; }

	public List<ProcessDevice>? MigrateOptions { get; set; }

	public List<int>? PrioritizedTypes { get; set; }

	private const int MigrateDiff = 2;

	#endregion

	#region Statistics Properties
	// * - Means that value must be divided by the modeling time
	// ** - Means that value must be divided by incoming elements count

	private double[] LastBusyCausesTimes { get; }

	private double LastOutTime { get; set; }

	private Dictionary<int, double> LastInTimesByType { get; }

	private double LastTime => Math.Max(LastOutTime,
		LastInTimesByType.Values.Count > 0 ? LastInTimesByType.Values.Max() : 0);

	public List<Element> Processed { get; }

	public int Rejected { get; private set; }

	public int Migrated { get; private set; }

	public double[] MeanLoads { get; } // *

	public double MeanInQueue { get; private set; } // *

	public Dictionary<int, List<double>> IncomingDeltas { get; } // **

	#endregion

	#region Constructor

	public ProcessDevice(string name, Func<double> distributionFunc, int maxQueue = int.MaxValue,
		int processorsCount = 1, List<int>? prioritizedTypes = null, int[]? startedQueue = null)
		: base(name, distributionFunc, processorsCount)
	{
		MaxQueue = maxQueue;
		PrioritizedTypes = prioritizedTypes;

		Queue = new PriorityQueue<Element, int>();
		Processed = new List<Element>();
		IncomingDeltas = new Dictionary<int, List<double>>();
		LastInTimesByType = new Dictionary<int, double>();
		LastBusyCausesTimes = new double[processorsCount];
		MeanLoads = new double[processorsCount];

		Array.Fill(NextTimes, double.MaxValue);

		States = new DeviceState[processorsCount];
		Array.Fill(States, DeviceState.Free);

		if (startedQueue == null) return;
		foreach (var elementType in startedQueue)
			PrioritizedEnqueue(new(elementType, 0));
		var inQueueNow = InQueue;
		for (var i = 0; i < inQueueNow && i < processorsCount; i++)
			NextTimes[i] = distributionFunc.Invoke();
	}

	#endregion

	public void InAction(double currentTime, Element element)
	{
		IncomingStatistics(element.Type, currentTime);

		if (InQueue >= MaxQueue)
		{
			Rejected++;
			ColoredConsole.WriteLine($"Rejected {this}", ConsoleColor.DarkRed);
			return;
		}
		var freeIndex = Array.IndexOf(States, DeviceState.Free);
		MeanInQueue += InQueue * (currentTime - LastTime);
		if (freeIndex != -1)
		{
			States[freeIndex] = DeviceState.Busy;
			NextTimes[freeIndex] = currentTime + DistributionFunc.Invoke();
			LastBusyCausesTimes[freeIndex] = currentTime;
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

		// For statistics example with 1 processor device
		// Busy:  ----------==========---===
		// Queue: 00000000000112223210000010
		if (InQueue > 0)
			NextTimes[processorI] = currentTime + DistributionFunc.Invoke();
		else
		{
			NextTimes[processorI] = double.MaxValue;
			States[processorI] = DeviceState.Free;
			MeanLoads[processorI] += currentTime - LastBusyCausesTimes[processorI];
		}

		MeanInQueue += InQueue * (currentTime - LastTime);

		for (var i = InQueue; i < 0; i++)
		{
			var freeIndex = Array.IndexOf(NextTimes, NextTimes.Where(t => t != double.MaxValue).Max());
			NextTimes[freeIndex] = double.MaxValue;
			States[freeIndex] = DeviceState.Free;
		}
		LastOutTime = currentTime;
		ColoredConsole.WriteLine($"Processed {this}", ConsoleColor.DarkGreen);

		var nextDevice = GetNextDevice(element.Type);
		if (nextDevice != null)
		{
			ColoredConsole.WriteLine($"Pass from {Name} to {nextDevice}", ConsoleColor.DarkGray);
			nextDevice.InAction(currentTime, element);
		}
		else element.OutOfSystemTime = currentTime;
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
		// It triggers IncomingStatistics. If it not needed pass bool parameters that turns off counting
	}

	private void PrioritizedEnqueue(Element element) => Queue.Enqueue(element, GetElementPriority(element));

	private int GetElementPriority(Element element) =>
		PrioritizedTypes != null && PrioritizedTypes.IndexOf(element.Type) != -1
			? PrioritizedTypes.IndexOf(element.Type) : int.MaxValue;

	private void IncomingStatistics(int type, double currentTime)
	{
		if (IncomingDeltas.TryGetValue(type, out var deltas)
		    && LastInTimesByType.TryGetValue(type, out var lastTime))
			deltas.Add(currentTime - lastTime);
		else if (IncomingDeltas.TryGetValue(type, out var deltasCase2))
			deltasCase2.Add(currentTime);
		else IncomingDeltas.Add(type, new List<double> { currentTime });

		if (LastInTimesByType.ContainsKey(type))
			LastInTimesByType[type] = currentTime;
		else LastInTimesByType.Add(type, currentTime);
	}

	public override string ToString() => $"{Name}: Next Times - {SC.StringifyList(NextTimes)}; " +
										 $"Processed - {SC.StringifyTypesCount(Processed)}; " +
										 $"In Queue - {SC.StringifyTypesCount(Queue.UnorderedItems.Select(t => t.Element).ToList())}";
}