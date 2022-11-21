using SystemModeling2.Devices;

namespace SystemModeling2.Model;

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

			foreach (var device in Devices)
				if (device is ProcessDevice processDevice)
					processDevice.TryMigrate(currentTime);
		}
		ShowStatistics(Devices, modelingTime);
	}

    public static void ShowStatistics(List<Device> devices, double modelingTime)
    {
	    var createDevices = GetSpecificDevices<CreateDevice>(devices);
	    var processDevices = GetSpecificDevices<ProcessDevice>(devices);

		Console.WriteLine("\n");
		foreach (var device in createDevices)
		    Console.WriteLine("Device " + device.Name + " Created: " + device.Finished);
	    Console.WriteLine("Sum of Created: " + createDevices.Sum(d => d.Finished) + "\n");

	    foreach (var device in processDevices)
		    Console.WriteLine("Device " + device.Name + " Processed: " + device.Finished);
		Console.WriteLine("Sum of Processed: " + processDevices.Sum(d => d.Finished) + "\n");

		var rejectedSum = processDevices.Sum(d => d.Rejected);
		foreach (var device in processDevices)
			Console.WriteLine("Device " + device.Name + " Rejected: " + device.Rejected);
		Console.WriteLine("Sum of Rejected: " + rejectedSum + " / " + rejectedSum / createDevices.Sum(d => d.Finished) + "%\n");

		foreach (var device in processDevices)
			Console.WriteLine("Device " + device.Name + " Migrated: " + device.Migrated);
		Console.WriteLine("Sum of Migrated: " + processDevices.Sum(d => d.Migrated) + "\n");

		foreach (var device in processDevices)
			Console.WriteLine("Device " + device.Name + " MeanBusyTime: " + device.MeanBusyTime);
		Console.WriteLine("Mean of MeanBusyTime: " + processDevices.Select(d => d.MeanBusyTime).Average() + 
		                  " with " + modelingTime + " total\n");

		foreach (var device in processDevices)
			Console.WriteLine("Device " + device.Name + " MeanInQueue: " + device.MeanInQueue / modelingTime);
		Console.WriteLine("Mean of MeanInQueue: " + processDevices.Select(d => d.MeanInQueue / modelingTime).Average());
    }

    private static List<T> GetSpecificDevices<T>(List<Device> devices) where T : Device
    {
	    var specificDevices = new List<T>();
	    foreach (var d in devices)
		    if (d is T cd)
			    specificDevices.Add(cd);
	    return specificDevices;
	}
}