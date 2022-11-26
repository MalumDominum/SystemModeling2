namespace SystemModeling2.Devices.Models;

public class Path
{
	public ProcessDevice Destination { get; set; }

	public int Priority { get; set; }

	public List<int>? PassTypes { get; set; }

	public Path(ProcessDevice destination, int priority = 1, List<int>? passTypes = null)
	{
		Destination = destination;
		Priority = priority;
		PassTypes = passTypes;
	}
}