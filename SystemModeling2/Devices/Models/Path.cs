using SystemModeling2.Devices.Enums;

namespace SystemModeling2.Devices.Models;

public class Path
{
	public ProcessDevice Destination { get; set; }

	public double PriorityOrChance { get; set; }

	public List<int>? PassTypes { get; set; }

	public Path(ProcessDevice destination, double priorityOrChance = 1, List<int>? passTypes = null)
	{
		Destination = destination;
		PriorityOrChance = priorityOrChance;
		PassTypes = passTypes;
	}
}