namespace SystemModeling2.Devices;

public class CreateDevice : Device
{
	public CreateDevice(string name, Func<double> distributionFunc, int processorsCount = 1)
		: base(name, distributionFunc, processorsCount)
	{
		for (var i = 0; i < NextTimes.Length; i++)
			NextTimes[i] = distributionFunc.Invoke();
	}

	public override void InAction(double currentTime) { }

	public override void OutAction(double currentTime)
	{
		Finished++;
		var processorIndex = Array.IndexOf(NextTimes, currentTime);
		NextTimes[processorIndex] = currentTime + DistributionFunc.Invoke();

		var nextDevice = GetNextDevice();
		if (nextDevice == null) return;
		Console.WriteLine($"Created to {nextDevice.Name} from {(NextTimes.Length > 1 ? $"[{processorIndex}]" : "")} {this}");
		nextDevice.InAction(currentTime);
	}
}