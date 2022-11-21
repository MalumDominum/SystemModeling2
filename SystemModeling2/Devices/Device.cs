using SystemModeling2.Devices.Enums;

namespace SystemModeling2.Devices;

public abstract class Device
{
	#region Properties

	public string Name { get; set; }

    public double NextTime { get; protected set; }

    public Func<double> DistributionFunc { get; init; }

    public List<(Device, int)>? NextPriorityTuples { get; set; }

    public int Finished { get; set; }

	private protected DeviceState State { get; set; }

	#endregion

	#region Constructor

	protected Device(string name, Func<double> distributionFunc)
    {
        Name = name;
        DistributionFunc = distributionFunc;

        NextTime = distributionFunc.Invoke();
		State = DeviceState.Free;
    }

	#endregion

	public abstract void InAction(double currentTime);

	public abstract void OutAction(double currentTime);

	private protected ProcessDevice? GetNextDevice()
	{
		if (NextPriorityTuples == null) return null;
		var processTuples = new List<(ProcessDevice, double)>();

		foreach (var tuple in NextPriorityTuples)
			if (tuple.Item1 is ProcessDevice processDevice)
				processTuples.Add((processDevice, tuple.Item2));

		var devicesWithMinQueue = processTuples.Where(t => t.Item1.InQueue == processTuples.Min(t => t.Item1.InQueue)).ToList();
		var nextDevice = devicesWithMinQueue.First(t => t.Item2 == devicesWithMinQueue.Max(t => t.Item2));
		return nextDevice.Item1;
	}

	public override string ToString() => $"{Name}: Next Time - {NextTime}, Finished - {Finished}";
}