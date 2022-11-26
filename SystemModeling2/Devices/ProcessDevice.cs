using SystemModeling2.Devices.Enums;
using SystemModeling2.Devices.Models;

namespace SystemModeling2.Devices;

public sealed class ProcessDevice : Device
{
	private const int MigrateDiff = 2;

	#region Properties

	private DeviceState[] States { get; }

	public List<ProcessDevice>? MigrateOptions { get; set; }

	public List<int>? PrioritizedTypes { get; set; }

	public int MaxQueue { get; init; }

	public PriorityQueue<int, int> Queue { get; }

	public int InQueue => Queue.Count;

	public int Rejected { get; private set; }

	public double MeanBusyTime { get; private set; }

	public double MeanInQueue { get; private set; } // Must be divided by the modeling time

	public int Migrated { get; private set; }

	private double PreviousTime { get; set; }

	public List<int> Processed { get; }

	#endregion

	#region Constructor

	public ProcessDevice(string name, Func<double> distributionFunc, int maxQueue = -1,
		int processorsCount = 1, List<int>? prioritizedTypes = null, StartedConditions? conditions = null)
		: base(name, distributionFunc, processorsCount)
	{
		MaxQueue = maxQueue;
		PrioritizedTypes = prioritizedTypes;
		Queue = new PriorityQueue<int, int>();
		Processed = new List<int>();

		Array.Fill(NextTimes, double.MaxValue);

		States = new DeviceState[processorsCount];
		Array.Fill(States, DeviceState.Free);

		for (var i = 0; i < processorsCount && conditions?.BusyCount != null && i < conditions.BusyCount; i++)
			States[i] = DeviceState.Busy;

		if (conditions?.InQueue == null) return;
		foreach (var elementType in conditions.InQueue)
			PrioritizedEnqueue(elementType);
		for (var i = 0; i < InQueue && i < processorsCount; i++)
			NextTimes[i] = distributionFunc.Invoke();
	}

	#endregion

	public override void InAction(double currentTime, int elementType = 1)
	{
		var freeIndex = Array.IndexOf(States, DeviceState.Free);
		if (freeIndex != -1)
		{
			States[freeIndex] = DeviceState.Busy;
			NextTimes[freeIndex] = currentTime + DistributionFunc.Invoke();
		}
		else
		{
			if (InQueue < MaxQueue || MaxQueue == -1)
			{
				PrioritizedEnqueue(elementType);
				ColoredConsole.WriteLine($"In Queue {this}", ConsoleColor.DarkYellow);
			}
			else
			{
				Rejected++;
				ColoredConsole.WriteLine($"Rejected {this}", ConsoleColor.DarkRed);
			}
		}
	}

	public override void OutAction(double currentTime, int elementType = 1)
	{
		var processorI = Array.IndexOf(NextTimes, currentTime);
		FinishedBy[processorI]++;
		ColoredConsole.WriteLine($"Processed {this}", ConsoleColor.DarkGreen);

		var nextDevice = GetNextDevice();
		if (nextDevice != null)
		{
			ColoredConsole.WriteLine($"Pass from {Name} to {nextDevice}", ConsoleColor.DarkGray);
			nextDevice.InAction(currentTime);
		}

		NextTimes[processorI] = double.MaxValue;
		PreviousTime = currentTime;

		if (InQueue > 0)
		{
			NextTimes[processorI] = currentTime + DistributionFunc.Invoke();
			MeanBusyTime += NextTimes[processorI] - currentTime;
			MeanInQueue += InQueue * (NextTimes[processorI] - currentTime);
			Processed.Add(Queue.Dequeue());
		}
		else States[processorI] = DeviceState.Free;
	}

	public void TryMigrate(double currentTime)
	{
		var toDevice = MigrateOptions?.Where(d => InQueue - d.InQueue >= MigrateDiff)
									  .FirstOrDefault(d => d.InQueue == MigrateOptions.Min(d => d.InQueue));
		if (toDevice == null) return;
		ColoredConsole.WriteLine($"Migrated to {toDevice.Name} (InQueue: {toDevice.InQueue}) " +
		                         $"from {Name} (InQueue: {InQueue})", ConsoleColor.DarkBlue);
		Migrated++;
		MeanInQueue -= currentTime - PreviousTime;
		toDevice.InAction(currentTime, Queue.Dequeue());
	}

	public void PrioritizedEnqueue(int elementType) => Queue.Enqueue(elementType, GetElementPriority(elementType));

	public int GetElementPriority(int elementType) => PrioritizedTypes != null
		? PrioritizedTypes.IndexOf(elementType) == -1 ? int.MaxValue : PrioritizedTypes.IndexOf(elementType)
		: 0;

	public string StringifyTypesCount(List<int> elementTypes) =>
		elementTypes.Distinct()
					.OrderBy(t => t)
					.Aggregate("", (current, type) =>
						$"{current}{(current == "" ? "" : ", ")}" +
						$"{type} = {elementTypes.Count(t => t == type)}");

	public string StringifyList(IEnumerable<double> list) => list.Select(t => t.ToString()).Aggregate((a, t) => $"{a}, {t}");

	public override string ToString() => $"{Name}: Next Times - {StringifyList(NextTimes)}; " +
	                                     $"Processed - {StringifyTypesCount(Processed)}; " +
	                                     $"In Queue - {StringifyTypesCount(Queue.UnorderedItems.Select(t => t.Element).ToList())}";
}