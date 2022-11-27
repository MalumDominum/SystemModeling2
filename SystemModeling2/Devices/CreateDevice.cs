using SystemModeling2.Devices.Models;

namespace SystemModeling2.Devices;

public class CreateDevice : Device
{
	public int CreatingType { get; init; }

	public static List<Element> AllElements { get; } = new();

	public CreateDevice(string name, Func<double> distributionFunc, int creatingType = 1, int processorsCount = 1,
		StartedConditions? conditions = null) : base(name, distributionFunc, processorsCount)
	{
		CreatingType = creatingType;

		if (conditions != null)
			Array.Fill(NextTimes, conditions.FirstInTime ?? distributionFunc.Invoke());
		else for (var i = 0; i < NextTimes.Length; i++)
			NextTimes[i] = distributionFunc.Invoke();
	}

	public override void OutAction(double currentTime)
	{
		var processorIndex = Array.IndexOf(NextTimes, currentTime);
		FinishedBy[processorIndex]++;
		NextTimes[processorIndex] = currentTime + DistributionFunc.Invoke();

		var nextDevice = GetNextDevice(CreatingType);
		if (nextDevice == null) return;
		
		var createdElement = new Element(CreatingType, currentTime);
		AllElements.Add(createdElement);
		Console.WriteLine($"Created to {nextDevice.Name} from {(NextTimes.Length > 1 ? $"[{processorIndex}] " : "")}{this}");
		nextDevice.InAction(currentTime, createdElement);
	}
}