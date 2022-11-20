using SystemModeling1.Devices;

namespace SystemModeling1.Model;

public class Model
{
    public List<Device> Devices { get; }

    public Model() => Devices = new List<Device>();

    public void Simulate(double modelingTime)
	{
		var currentTime = 0.0;

		while (currentTime < modelingTime)
		{
			var nextTime = Devices.Min(d => d.NextTime);
			if (nextTime > modelingTime) break;
			var nextDevices = Devices.Where(d => d.NextTime == nextTime);
			currentTime = nextTime;

			foreach (var device in nextDevices)
                device.OutAction(currentTime);
		}
		ShowStatistics(Devices);
	}

    public static void ShowStatistics(List<Device> devices)
    {
		var createDevices = new List<CreateDevice>();
		foreach (var d in devices)
			if (d is CreateDevice cd)
				createDevices.Add(cd);
		var processDevices = new List<ProcessDevice>();
		foreach (var d in devices)
			if (d is ProcessDevice pd)
				processDevices.Add(pd);

		Console.WriteLine("\n");
		foreach (var device in createDevices)
		    Console.WriteLine("Device " + device.Name + " Created: " + device.Finished);
	    Console.WriteLine("Sum of Created: " + createDevices.Sum(d => d.Finished) + "\n");

	    foreach (var device in processDevices)
		    Console.WriteLine("Device " + device.Name + " Processed: " + device.Finished);
		Console.WriteLine("Sum of Processed: " + processDevices.Sum(d => d.Finished) + "\n");

		foreach (var device in processDevices)
			Console.WriteLine("Device " + device.Name + " Rejected: " + device.Rejected);
		Console.WriteLine("Sum of Rejected: " + processDevices.Sum(d => d.Rejected) + "\n");
    }
}