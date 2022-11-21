using SystemModeling2.Devices.Enums;

namespace SystemModeling2.Devices;

public abstract class Device
{
	#region Properties

	public string Name { get; set; }

    public double[] NextTimes { get; protected set; }

	public Func<double> DistributionFunc { get; init; }

    public List<(Device, int)>? NextPriorityTuples { get; set; }

    public int Finished { get; set; }

	private protected DeviceState State { get; set; }

	#endregion

	#region Constructor

	protected Device(string name, Func<double> distributionFunc, int processorsCount = 1)
    {
        Name = name;
        DistributionFunc = distributionFunc;
		State = DeviceState.Free;
		NextTimes = new double[processorsCount];
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

	public override string ToString() => $"{Name}: Next Times - {NextTimes.Select(t => t.ToString()).Aggregate((a, t) => $"{a} {t}")}, " +
	                                     $"Finished - {Finished}";
}