using SystemModeling1.Devices.Enums;

namespace SystemModeling1.Devices;

public sealed class ProcessDevice : Device
{
	#region Properties

	public int MaxQueue { get; init; }

	public int InQueue { get; private set; }

	public int Rejected { get; private set; }

	//private double meanQueue;

	#endregion

	public ProcessDevice(string name, Func<double> distributionFunc,
		int maxQueue = -1, List<(Device, int)>? nextPriorityTuples = null)
		: base(name, distributionFunc, nextPriorityTuples)
	{
		MaxQueue = maxQueue;
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
				ColoredConsole.WriteLine($"In Queue {this}", ConsoleColor.DarkYellow);
				InQueue++;
			}
			else
			{
				ColoredConsole.WriteLine($"Rejected {this}", ConsoleColor.DarkRed);
				Rejected++;
			}
		}
	}

	public override void OutAction(double currentTime)
	{
		if (State != DeviceState.Free)
		{
			Finished++;
			ColoredConsole.WriteLine($"Processed {this}", ConsoleColor.DarkGreen);

			var nextDevice = GetNextDevice();
			if (nextDevice != null)
			{
				ColoredConsole.WriteLine($"Pass from {Name} to {nextDevice}", ConsoleColor.DarkGray);
				nextDevice.InAction(currentTime);
			}
		}
		State = DeviceState.Free;
		NextTime = double.MaxValue;

		if (InQueue <= 0) return;
		NextTime = currentTime + DistributionFunc.Invoke();
		InQueue--;
	}

	public override string ToString() => $"{Name}: State - {State}, Next Time - {NextTime}, " +
	                                     $"Finished - {Finished}, In Queue - {InQueue}";
}