using Path = SystemModeling2.Devices.Models.Path;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;

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
	
	public abstract void OutAction(double currentTime);

	private protected ProcessDevice? GetNextDevice(int elementType)
	{
		if (Paths == null) return null;

		var pathsCanBePassed = Paths.Where(p => p.PassTypes == null || (p.PassTypes != null && p.PassTypes.Contains(elementType))).ToList();

		var pathsWithMinQueue = pathsCanBePassed.Where(p => p.Destination.InQueue == pathsCanBePassed.Min(path => path.Destination.InQueue));
		return pathsWithMinQueue.MinBy(p => p.Priority)?.Destination;
	}
}