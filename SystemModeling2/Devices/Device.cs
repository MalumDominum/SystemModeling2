using Path = SystemModeling2.Devices.Models.Path;

namespace SystemModeling2.Devices;

public abstract class Device
{
	#region Properties

	public string Name { get; set; }

    public double[] NextTimes { get; }

	public Func<double> DistributionFunc { get; init; }

    public List<Path>? Paths { get; set; }

	public int[] FinishedBy { get; set; }

	public int Finished => FinishedBy.Sum();

	#endregion

	#region Constructor

	protected Device(string name, Func<double> distributionFunc, int processorsCount = 1)
    {
        Name = name;
        DistributionFunc = distributionFunc;
		NextTimes = new double[processorsCount];
		FinishedBy = new int[processorsCount];
    }

	#endregion

	public abstract void InAction(double currentTime, int elementType = 1);

	public abstract void OutAction(double currentTime, int elementType = 1);

	private protected ProcessDevice? GetNextDevice()
	{
		if (Paths == null) return null;
		var processTuples = new List<(ProcessDevice, double)>();

		foreach (var path in Paths)
			if (path.Destination is ProcessDevice processDevice)
				processTuples.Add((processDevice, path.Priority)); // TODO

		var devicesWithMinQueue = processTuples.Where(t => t.Item1.InQueue == processTuples.Min(t => t.Item1.InQueue)).ToList();
		var nextDevice = devicesWithMinQueue.First(t => t.Item2 == devicesWithMinQueue.Min(t => t.Item2));
		return nextDevice.Item1;
	}

	public override string ToString() => $"{Name}: Next Times - {NextTimes.Select(t => t.ToString()).Aggregate((a, t) => $"{a} {t}")}, " +
	                                     $"Finished - {Finished}";
}