namespace SystemModeling2.Devices;

public class CreateDevice : Device
{
	public override void InAction(double currentTime) { }

	public override void OutAction(double currentTime)
	{
		Finished++;
		NextTime = currentTime + DistributionFunc.Invoke();

		var nextDevice = GetNextDevice();
		if (nextDevice == null) return;
		Console.WriteLine($"Created to {nextDevice.Name} from {this}");
		nextDevice.InAction(currentTime);
	}

	public CreateDevice(string name, Func<double> distributionFunc, List<(Device, int)>? nextPriorityTuples = null)
		: base(name, distributionFunc, nextPriorityTuples) { }
}