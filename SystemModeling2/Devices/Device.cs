using SystemModeling2.Devices.Enums;
using SystemModeling2.Devices.Models;
using RE = SystemModeling2.Infrastructure.RandomExtended;

namespace SystemModeling2.Devices;

public abstract class Device
{
	#region Properties

	public string Name { get; set; }

	public bool StatisticsCollectionDisabled { get; set; }

    public double[] NextTimes { get; }

	public RE? Rnd { get; }

	public Func<RE?, double> DistributionFunc { get; init; }

    public PathGroup PathGroup { get; set; }

    public int ProcessorsCount => NextTimes.Length;

    public int[] FinishedBy { get; set; }

	public int Finished => FinishedBy.Sum();

	#endregion

	#region Constructor

	protected Device(string name, Func<RE?, double> distributionFunc, RE? rnd = null, int processorsCount = 1)
    {
        Name = name;
        Rnd = rnd;
        DistributionFunc = distributionFunc;
		NextTimes = new double[processorsCount];
		FinishedBy = new int[processorsCount];
    }

	#endregion

    public double DistributionInvoke() => DistributionFunc(Rnd);

	public abstract void OutAction(double currentTime);

	private protected ProcessDevice? GetNextDevice(int elementType)
	{
		if (PathGroup == null) return null;

		var pathsCanBePassed = PathGroup.Paths.Where(p => p.PassTypes == null || (p.PassTypes != null && p.PassTypes.Contains(elementType))).ToList();

		switch (PathGroup.SelectionPath)
		{
			case SelectionPath.Priority:
				var pathsWithFreeQueue = pathsCanBePassed.Where(p => p.Destination.InQueue < p.Destination.MaxQueue).ToList();
				return pathsWithFreeQueue.Count > 0
					? pathsWithFreeQueue.MinBy(p => p.PriorityOrChance)?.Destination
					: pathsCanBePassed.MinBy(p => p.PriorityOrChance)?.Destination;
			case SelectionPath.UniformPriority:
				var pathsWithMinQueue = pathsCanBePassed.Where(p => p.Destination.InQueue == pathsCanBePassed.Min(path => path.Destination.InQueue));
				return pathsWithMinQueue.MinBy(p => p.PriorityOrChance)?.Destination;
			case SelectionPath.Random:
				if (pathsCanBePassed.Sum(p => p.PriorityOrChance) > 1)
					throw new ArgumentException("Chances can't be bigger that 1");

				var randomValue = new Random().NextDouble();
				ProcessDevice? result = null;
				foreach (var t in pathsCanBePassed)
				{
					randomValue -= t.PriorityOrChance;
					if (randomValue > 0) continue;

					result = t.Destination;
					break;
				}
				return result;
			default:
				throw new NullReferenceException("Selection path not choosen for the group");
		}
    }

    public virtual void Reset() => Array.Fill(FinishedBy, 0);
}