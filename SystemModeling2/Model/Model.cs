using SystemModeling2.Devices;
using SystemModeling2.Devices.Models;
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
		CreateDevice.AllElements.Clear();
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

	    var processedAverage = processDevices.Average(d => d.Finished);
		foreach (var device in processDevices)
		    Console.WriteLine($"Device {device.Name} Processed: {SC.StringifyTypesCount(device.Processed)}, " +
		                      $"Sum: {device.Finished}; MeanProcessingTime: {device.Finished / modelingTime}");
		Console.WriteLine($"Mean of Processed: {processedAverage}, " +
		                  $"MeanProcessingTime: {processedAverage / modelingTime}\n");

		var rejectedSum = processDevices.Sum(d => d.Rejected);
		if (processDevices.Any(d => d.Rejected > 0))
		{
			foreach (var device in processDevices)
				Console.WriteLine($"Device {device.Name} Rejected: {device.Rejected} | That is {Math.Round((double)device.Rejected / device.Finished * 100, 2)}% of Processed");
			Console.WriteLine($"Sum of Rejected: {rejectedSum} | {Math.Round((double)rejectedSum / createdSum * 100, 2)}% chance of Reject\n");
	    }
	    else ColoredConsole.WriteLine("Devices doesn't reject elements\n", ConsoleColor.DarkGray);

		if (processDevices.Any(d => d.Migrated > 0))
		{
			foreach (var device in processDevices)
				Console.WriteLine($"Device {device.Name} Migrated:\t{device.Migrated}");
			Console.WriteLine($"Sum of Migrated: {processDevices.Sum(d => d.Migrated)}\n");
		}
		else ColoredConsole.WriteLine("Elements doesn't migrated between devices\n", ConsoleColor.DarkGray);

		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanBusyTime: {Math.Round(device.MeanBusyTime, 6)}");
		Console.WriteLine($"MeanBusyTime of all: {Math.Round(processDevices.Select(d => d.MeanBusyTime).Average(), 6)} " +
		                  $"with {modelingTime} total\n");
		
		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanInQueue: {Math.Round(device.MeanInQueue / modelingTime, 6)}");
		Console.WriteLine($"MeanInQueue of all: {Math.Round(processDevices.Select(d => d.MeanInQueue / modelingTime).Average(), 6)}\n");

		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanIncomingTime: {SC.StringifyDict(device.IncomingTimes, modelingTime)}");
		Console.WriteLine($"MeanIncomingTime of all: {Math.Round(processDevices.Average(d => d.IncomingTimes.Average(x => x.Value / modelingTime)), 4)}\n");

		foreach (var type in CreateDevice.AllElements.Select(e => e.Type).Distinct().Order())
			Console.WriteLine($"Element type {type}: MeanLiveTime - " +
			                  $"{Math.Round((double)CreateDevice.AllElements.Where(e => e.Type == type).Average(e => e.LiveTime), 6)}");
		Console.WriteLine($"MeanLiveTime of all: {Math.Round((double)CreateDevice.AllElements.Select(e => e.LiveTime).Average(), 6)}\n");
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