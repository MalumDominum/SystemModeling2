using SystemModeling2.Devices.Enums;

namespace SystemModeling2.Devices;

public sealed class ProcessDevice : Device
{
	private const int MigrateDiff = 2;

	#region Properties

	private DeviceState[] States { get; }

	public List<ProcessDevice>? MigrateOptions { get; set; }

	public int MaxQueue { get; init; }

	public int InQueue { get; private set; }

	public int Rejected { get; private set; }

	public double MeanBusyTime { get; private set; }

	public double MeanInQueue { get; private set; } // Must be divided by the modeling time

	public int Migrated { get; private set; }

	private double PreviousTime { get; set; }

	#endregion

	#region Constructor

	public ProcessDevice(string name, Func<double> distributionFunc, int maxQueue = -1,
		int processorsCount = 1, StartedConditions? conditions = null) : base(name, distributionFunc, processorsCount)
	{
		MaxQueue = maxQueue;

		Array.Fill(NextTimes, double.MaxValue);

		States = new DeviceState[processorsCount];
		Array.Fill(States, DeviceState.Free);

		for (var i = 0; i < processorsCount && conditions?.BusyCount != null && i < conditions.BusyCount; i++)
			States[i] = DeviceState.Busy;

		if (conditions?.InQueue == null) return;
		InQueue = (int)conditions.InQueue;
		for (var i = 0; i < InQueue && i < processorsCount; i++)
			NextTimes[i] = distributionFunc.Invoke();
	}

	#endregion

	public override void InAction(double currentTime)
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
				InQueue++;
				ColoredConsole.WriteLine($"In Queue {this}", ConsoleColor.DarkYellow);
			}
			else
			{
				Rejected++;
				ColoredConsole.WriteLine($"Rejected {this}", ConsoleColor.DarkRed);
			}
		}
	}

	public override void OutAction(double currentTime)
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
			InQueue--;
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
		InQueue--;
		MeanInQueue -= currentTime - PreviousTime;
		toDevice.InAction(currentTime);
	}

	public override string ToString() => $"{Name}: Next Times - {NextTimes.Select(t => t.ToString())
										 .Aggregate((a, t) => $"{a} {t}")}, Finished - {Finished}, In Queue - {InQueue}";
}