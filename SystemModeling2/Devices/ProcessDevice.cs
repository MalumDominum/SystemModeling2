using SystemModeling2.Devices.Enums;

namespace SystemModeling2.Devices;

public sealed class ProcessDevice : Device
{
	#region Properties

	public int MaxQueue { get; init; }

	public int InQueue { get; private set; }

	public int Rejected { get; private set; }

	public double MeanInQueueTime { get; private set; }

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
		State = DeviceState.Free;
		NextTime = double.MaxValue;

		if (InQueue <= 0) return;
		NextTime = currentTime + DistributionFunc.Invoke();
		MeanInQueueTime += InQueue * (NextTime - currentTime);
		InQueue--;
	}

	public override string ToString() => $"{Name}: Next Time - {NextTime}, Mean in Queue Time - {MeanInQueueTime} " +
	                                     $"Finished - {Finished}, In Queue - {InQueue}";
}