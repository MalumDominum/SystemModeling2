﻿using SystemModeling2.Devices;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;

namespace SystemModeling2.Model;

public class Model
{
    public List<Device> Devices { get; }

    public Model() => Devices = new List<Device>();

    public void Simulate(double modelingTime)
	{
		var currentTime = 0.0;
		var createDevices = GetSpecificDevices<CreateDevice>(Devices);
		var processDevices = GetSpecificDevices<ProcessDevice>(Devices);

		while (currentTime < modelingTime)
		{
			var nextTime = Devices.Min(d => d.NextTimes.Min());
			if (nextTime > modelingTime) break;
			var nextDevices = Devices.Where(d => d.NextTimes.Contains(nextTime));

			foreach (var device in processDevices)
				device.DoStatistics(Math.Min(nextTime, modelingTime) - currentTime);

			currentTime = nextTime;

			foreach (var device in nextDevices)
                device.OutAction(currentTime);

			foreach (var device in Devices)
				if (device is ProcessDevice processDevice)
					processDevice.TryMigrate(currentTime);
		}
		ShowStatistics(createDevices, processDevices, modelingTime);
		CreateDevice.AllElements.Clear();
	}

    private static void ShowStatistics(List<CreateDevice> createDevices,
	    List<ProcessDevice> processDevices, double modelingTime)
    {
		Console.WriteLine("\n");
		var createdSum = createDevices.Sum(d => d.Finished);
		foreach (var device in createDevices)
		    Console.WriteLine($"Device {device.Name} Created: {device.Finished} of type {device.CreatingType}");
	    Console.WriteLine($"Sum of Created: {createdSum}\n");

		foreach (var device in processDevices)
		    Console.WriteLine($"Device {device.Name} Processed: {SC.StringifyTypesCount(device.Processed)}, " +
		                      $"Sum: {device.Finished}; BusyTime: {device.BusyTime}; " +
		                      $"MeanProcessingTime: {device.Finished / device.BusyTime}");
		Console.WriteLine($"Avg of Processed: {processDevices.Average(d => d.Finished)}, " +
		                  $"Avg of MeanProcessingTime: {processDevices.Average(d => d.Finished / d.BusyTime)}\n");

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
				Console.WriteLine($"Device {device.Name} Migrated: {device.Migrated}");
			Console.WriteLine($"Sum of Migrated: {processDevices.Sum(d => d.Migrated)}\n");
		}
		else ColoredConsole.WriteLine("Elements doesn't migrated between devices\n", ConsoleColor.DarkGray);

		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanLoad: {device.BusyTime / modelingTime}");
		Console.WriteLine($"Average of MeanLoad: {Math.Round(processDevices.Select(d => d.BusyTime / modelingTime).Average(), 6)}\n");

		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanInQueue: {Math.Round(device.MeanInQueue / modelingTime, 6)}");
		Console.WriteLine($"Average of MeanInQueue: {Math.Round(processDevices.Select(d => d.MeanInQueue / modelingTime).Average(), 6)}\n");

		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanIncomingTime: {SC.StringifyDictOfLists(device.IncomingDeltas)}");
		Console.WriteLine($"Average of MeanIncomingTime: {Math.Round(processDevices.Average(d => d.IncomingDeltas.Sum(x => x.Value.Average())), 4)}\n");

		foreach (var type in CreateDevice.AllElements.Select(e => e.Type).Distinct().Order())
			Console.WriteLine($"Element type {type}: MeanLiveTime - " +
			                  $"{Math.Round(CreateDevice.AllElements.Where(e => e.Type == type).Average(e => e.LiveTime)
			                                ?? double.PositiveInfinity, 6)}");
		Console.WriteLine($"Average of MeanLiveTime: {Math.Round(CreateDevice.AllElements.Select(e => e.LiveTime).Average()
		                                                         ?? double.PositiveInfinity, 6)}\n");
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