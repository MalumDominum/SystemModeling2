namespace SystemModeling1.Devices;

public class CreateDevice : Device
{
	public override void InAction(double currentTime)
	{

	}

	public override void OutAction(double currentTime)
	{
		Finished++;
		NextTime = currentTime + DistributionFunc.Invoke();

		var nextDevice = GetNextDevice();
		if (nextDevice == null) return;
		nextDevice.InAction(currentTime);
		Console.WriteLine($"Created to {nextDevice.Name} from {this}");
	}


	public CreateDevice(string name, Func<double> distributionFunc, List<(Device, int)>? nextPriorityTuples = null)
		: base(name, distributionFunc, nextPriorityTuples) { }
}