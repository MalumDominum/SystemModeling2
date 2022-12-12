namespace SystemModeling2.Devices.Models;

public class MigrateOption
{
	public ProcessDevice Destination { get; set; }
	
	public int Difference { get; set; }

	public MigrateOption(ProcessDevice destination, int difference)
	{
		Destination = destination;
		Difference = difference;
	}
}