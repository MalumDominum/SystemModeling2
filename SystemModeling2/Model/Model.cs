using SystemModeling2.Devices;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;

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
			var nextTime = Devices.Min(d => d.NextTimes.Min());
			if (nextTime > modelingTime) break;
			var nextDevices = Devices.Where(d => d.NextTimes.Contains(nextTime));
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
		var createdSum = createDevices.Sum(d => d.Finished);
		foreach (var device in createDevices)
		    Console.WriteLine($"Device {device.Name} Created: {device.Finished} of type {device.CreatingType}");
	    Console.WriteLine($"Sum of Created: {createdSum}\n");
		
		foreach (var device in processDevices)
		    Console.WriteLine($"Device {device.Name} Processed: {SC.StringifyTypesCount(device.Processed)}, Sum: {device.Finished}");
		Console.WriteLine($"Sum of Processed: {processDevices.Sum(d => d.Finished)}\n");

		var rejectedSum = processDevices.Sum(d => d.Rejected);
		if (processDevices.Any(d => d.Migrated > 0))
		{
			foreach (var device in processDevices)
				Console.WriteLine($"Device {device.Name} Rejected: {device.Rejected}");
			Console.WriteLine($"Sum of Rejected: {rejectedSum} / {Math.Round((double)rejectedSum / createdSum * 100, 2)}%\n");
	    }
	    else ColoredConsole.WriteLine("Devices doesn't reject elements\n", ConsoleColor.DarkGray);

		if (processDevices.Any(d => d.Migrated > 0))
		{
			foreach (var device in processDevices)
				Console.WriteLine($"Device {device.Name} Migrated: {device.Migrated}");
			Console.WriteLine($"Sum of Migrated: {processDevices.Sum(d => d.Migrated)}\n");
		}
		else ColoredConsole.WriteLine("Elements doesn't migrated between devices\n", ConsoleColor.DarkGray);

		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanBusyTime: {device.MeanBusyTime}");
		Console.WriteLine($"Mean of MeanBusyTime: {processDevices.Select(d => d.MeanBusyTime).Average()} with {modelingTime} total\n");
		
		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanInQueue: {device.MeanInQueue / modelingTime}");
		Console.WriteLine($"Mean of MeanInQueue: {processDevices.Select(d => d.MeanInQueue / modelingTime).Average()}");
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