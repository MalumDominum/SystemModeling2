using SystemModeling2.Devices.Enums;

namespace SystemModeling2.Devices;

public sealed class ProcessDevice : Device
{
	private const int MigrateDiff = 2;

	#region Properties

	public List<ProcessDevice>? MigrateOptions { get; set; }
	
	public int MaxQueue { get; init; }

	public int InQueue { get; private set; }

	public int Rejected { get; private set; }

	public double MeanBusyTime { get; private set; }

	public double MeanInQueue { get; private set; } // Must be divided by the modeling time

	public int Migrated { get; private set; }

	private double PreviousTime { get; set; }

	#endregion

	public ProcessDevice(string name, Func<double> distributionFunc,
		int maxQueue = -1, List<(Device, int)>? nextPriorityTuples = null)
		: base(name, distributionFunc, nextPriorityTuples)
	{
		MaxQueue = maxQueue;
		NextTime = double.MaxValue;
	}

	public override void InAction(double currentTime)
	{
		if (State == DeviceState.Free)
		{
			State = DeviceState.Busy;
			NextTime = currentTime + DistributionFunc.Invoke();
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
		Finished++;
		ColoredConsole.WriteLine($"Processed {this}", ConsoleColor.DarkGreen);

		var nextDevice = GetNextDevice();
		if (nextDevice != null)
		{
			ColoredConsole.WriteLine($"Pass from {Name} to {nextDevice}", ConsoleColor.DarkGray);
			nextDevice.InAction(currentTime);
		}
		NextTime = double.MaxValue;
		PreviousTime = currentTime;

		if (InQueue > 0)
		{
			NextTime = currentTime + DistributionFunc.Invoke();
			MeanBusyTime += NextTime - currentTime;
			MeanInQueue += InQueue * (NextTime - currentTime);
			InQueue--;
		}
		else State = DeviceState.Free;
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

	public override string ToString() => $"{Name}: Next Time - {NextTime}, Finished - {Finished}, In Queue - {InQueue}";
}