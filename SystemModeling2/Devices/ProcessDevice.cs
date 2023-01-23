using SystemModeling2.Devices.Enums;
using SystemModeling2.Devices.Models;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;
using RE = SystemModeling2.Infrastructure.RandomExtended;

namespace SystemModeling2.Devices;

public sealed class ProcessDevice : Device
{
	#region Logic Properties

	public PriorityQueue<Element, int> Queue { get; }

	public int BusyProcessors => States.Count(t => t == DeviceState.Busy);

	public int InQueue => Queue.Count - BusyProcessors;

	public int MaxQueue { get; init; }

	private DeviceState[] States { get; }

	public List<MigrateOption>? MigrateOptions { get; set; }

	public List<int>? PrioritizedTypes { get; set; }

	public int[]? StartedQueue { get; }

    #endregion

	#region Statistics Properties
	// * - Means that value must be divided by the modeling time
	// ** - Means that value must be divided by incoming elements count

	private Dictionary<int, double> LastInTimesByType { get; }

	public List<Element> Processed { get; }

	public int Rejected { get; private set; }

	public int Migrated { get; private set; }

	public double BusyTime { get; private set; } // *

	public double MeanInQueue { get; private set; } // *

	public Dictionary<int, List<double>> IncomingDeltas { get; } // **

	#endregion

	#region Constructor

	public ProcessDevice(string name, Func<RE?, double> distributionFunc, RE? rnd = null, int maxQueue = int.MaxValue,
		int processorsCount = 1, List<int>? prioritizedTypes = null, int[]? startedQueue = null)
		: base(name, distributionFunc, rnd, processorsCount)
	{
		MaxQueue = maxQueue;
		PrioritizedTypes = prioritizedTypes;
        StartedQueue = startedQueue;

        Queue = new PriorityQueue<Element, int>();
		Processed = new List<Element>();
		IncomingDeltas = new Dictionary<int, List<double>>();
		LastInTimesByType = new Dictionary<int, double>();

		Array.Fill(NextTimes, double.MaxValue);

		States = new DeviceState[processorsCount];
		Array.Fill(States, DeviceState.Free);

		if (startedQueue == null) return;
		foreach (var elementType in startedQueue)
			PrioritizedEnqueue(new(elementType, 0));
		var inQueueNow = InQueue;
		for (var i = 0; i < inQueueNow && i < processorsCount; i++)
			NextTimes[i] = DistributionInvoke();
	}

	#endregion

	public void InAction(double currentTime, Element element)
	{
		IncomingStatistics(element.Type, currentTime);

		var freeIndex = Array.IndexOf(States, DeviceState.Free);
		if (freeIndex != -1)
		{
			States[freeIndex] = DeviceState.Busy;
			NextTimes[freeIndex] = currentTime + DistributionInvoke();
			PrioritizedEnqueue(element);
		}
		else if (InQueue >= MaxQueue)
		{
			Rejected++;
			ColoredConsole.WriteLine($"Rejected {this}", ConsoleColor.DarkRed);
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

		if (InQueue >= 0)
			NextTimes[processorI] = currentTime + DistributionInvoke();
		else
		{
			NextTimes[processorI] = double.MaxValue;
			States[processorI] = DeviceState.Free;
		}
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
		var minQueue = MigrateOptions?.Min(option => option.Destination.InQueue);
		var toDevice = MigrateOptions?.Where(mo => InQueue - mo.Destination.InQueue >= mo.Difference)
									  .FirstOrDefault(mo => mo.Destination.InQueue == minQueue)
									       ?.Destination;
		if (toDevice == null) return;
		ColoredConsole.WriteLine($"Migrated to {toDevice.Name} (InQueue: {toDevice.InQueue}) " +
								 $"from {Name} (InQueue: {InQueue})", ConsoleColor.DarkBlue);
		Migrated++;
		toDevice.InAction(currentTime, Queue.Dequeue());
	}

	public void DoStatistics(double delta)
	{
		BusyTime += States.Average(s => (double)s) * delta;
		MeanInQueue += InQueue * delta;
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
										 $"Queue - {SC.StringifyTypesCount(Queue.UnorderedItems.Select(t => t.Element).ToList())}";

	public override void Reset()
	{
		base.Reset();
		
        Queue.Clear();
        Processed.Clear();
        IncomingDeltas.Clear();
        LastInTimesByType.Clear();

        Array.Fill(NextTimes, double.MaxValue);
        Array.Fill(States, DeviceState.Free);

        if (StartedQueue == null) return;
        foreach (var elementType in StartedQueue)
            PrioritizedEnqueue(new(elementType, 0));
        var inQueueNow = InQueue;
        for (var i = 0; i < inQueueNow && i < ProcessorsCount; i++)
            NextTimes[i] = DistributionInvoke();
    }
}