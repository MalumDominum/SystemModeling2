using SystemModeling2.Devices.Models;
using SystemModeling2.Infrastructure;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;
using RE = SystemModeling2.Infrastructure.RandomExtended;

namespace SystemModeling2.Devices;

public class CreateDevice : Device
{
	public int CreatingType { get; init; }

	public double? FirstCreatingTime { get; }

	public static List<Element> AllElements { get; } = new();

	public CreateDevice(string name, Func<RE?, double> distributionFunc, RE? rnd = null, int creatingType = 1, int processorsCount = 1,
		double? firstCreatingTime = null) : base(name, distributionFunc, rnd, processorsCount)
	{
		CreatingType = creatingType;
		FirstCreatingTime = firstCreatingTime;

		if (firstCreatingTime != null)
			Array.Fill(NextTimes, (double)firstCreatingTime);
		else for (var i = 0; i < NextTimes.Length; i++)
			NextTimes[i] = DistributionInvoke();
	}

	public override void OutAction(double currentTime)
	{
		var processorIndex = Array.IndexOf(NextTimes, currentTime);

        NextTimes[processorIndex] = currentTime + DistributionInvoke();

		var nextDevice = GetNextDevice(CreatingType);
		if (nextDevice == null) return;
		
		var createdElement = new Element(CreatingType, currentTime);
        if (!StatisticsCollectionDisabled)
        {
            FinishedBy[processorIndex]++;
            AllElements.Add(createdElement);
        }
		ColoredConsole.WriteLine($"Created to {nextDevice.Name} from " +
                                 $"{(NextTimes.Length > 1 ? $"[{processorIndex}] " : "")}{this}", ConsoleColor.White);
		nextDevice.InAction(currentTime, createdElement);
	}

	public override string ToString() => $"{Name}: Next Times - {SC.StringifyList(NextTimes)}; Created - {Finished}";

    public override void Reset()
	{
        base.Reset();

        if (FirstCreatingTime.HasValue)
            Array.Fill(NextTimes, FirstCreatingTime.Value);
        else for (var i = 0; i < NextTimes.Length; i++)
            NextTimes[i] = DistributionInvoke();
    }
}