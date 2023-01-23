using SystemModeling2.Devices;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;

namespace SystemModeling2.Model;

public static class ModelSimulator
{
    private static double? RunSimulationStep(Model model, double currentTime, double modelingTime)
    {
        var nextTime = model.Devices.Min(d => d.NextTimes.Min());
        if (nextTime > currentTime) return null;
        var nextDevices = model.Devices.Where(d => d.NextTimes.Contains(nextTime));

        foreach (var device in model.ProcessDevices)
            device.DoStatistics(Math.Min(nextTime, modelingTime) - currentTime);

        currentTime = nextTime;

        foreach (var device in nextDevices)
            device.OutAction(currentTime);

        foreach (var device in model.Devices)
            if (device is ProcessDevice processDevice)
                processDevice.TryMigrate(currentTime);

		return nextTime;
    }

    public static void RunSimulate(Model model, double modelingTime)
	{
		var currentTime = 0.0;

        while (currentTime < modelingTime)
            currentTime = RunSimulationStep(model, currentTime, modelingTime) ?? double.MaxValue;

		ShowStatistics(model.CreateDevices, model.ProcessDevices, modelingTime);
		CreateDevice.AllElements.Clear();
    }
	
    public static void RunExperiment(List<Model> models, double modelingTime, double numberOfSimulations)
    {
		var rnd = new Random();
        var seedSequence = new List<int>();
        for (int i = 0; i < numberOfSimulations; i++)
            seedSequence.Add(rnd.Next());

        for (int modelI = 0; modelI < models.Count; modelI++)
        {
            for (int currentSimulation = 0; currentSimulation < numberOfSimulations; currentSimulation++)
            {
                RunSimulate(models[modelI], modelingTime);
				models[modelI].Clear(seedSequence[currentSimulation]);
            }
        }
    }

    private static double Round(double value, int digits = 5) => Math.Round(value, digits);

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
		                      $"Sum: {device.Finished}; BusyTime: {Round(device.BusyTime)}; " +
		                      $"MeanProcessingTime: {Round(device.Finished / device.BusyTime)}");
		Console.WriteLine($"Avg of Processed: {processDevices.Average(d => d.Finished)}, " +
		                  $"Avg of MeanProcessingTime: {Round(processDevices.Average(d => d.Finished / d.BusyTime))}\n");

		var rejectedSum = processDevices.Sum(d => d.Rejected);
		if (processDevices.Any(d => d.Rejected > 0))
		{
			foreach (var device in processDevices)
				Console.WriteLine($"Device {device.Name} Rejected: {device.Rejected} | That is {Round((double)device.Rejected / device.Finished * 100)}% of Processed");
			Console.WriteLine($"Sum of Rejected: {rejectedSum} | {Round((double)rejectedSum / createdSum * 100)}% chance of Reject\n");
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
			Console.WriteLine($"Device {device.Name} MeanLoad: {Round(device.BusyTime / modelingTime)}");
		Console.WriteLine($"Average of MeanLoad: {Round(processDevices.Select(d => d.BusyTime / modelingTime).Average())}\n");

		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanInQueue: {Round(device.MeanInQueue / modelingTime)}");
		Console.WriteLine($"Average of MeanInQueue: {Round(processDevices.Select(d => d.MeanInQueue / modelingTime).Average())}\n");

		foreach (var device in processDevices)
			Console.WriteLine($"Device {device.Name} MeanIncomingInterval: {SC.StringifyDictOfLists(device.IncomingDeltas)}");
		Console.WriteLine($"Average of MeanIncomingInterval: {Round(processDevices.Average(d => d.IncomingDeltas.Sum(x => x.Value.Average())))}\n");

		foreach (var type in CreateDevice.AllElements.Select(e => e.Type).Distinct().Order())
			Console.WriteLine($"Element type {type}: MeanLiveTime - " +
			                  $"{Round(CreateDevice.AllElements.Where(e => e.Type == type).Average(e => e.LiveTime)
			                                ?? double.PositiveInfinity)}");
		Console.WriteLine($"Average of MeanLiveTime: {Round(CreateDevice.AllElements.Select(e => e.LiveTime).Average()
		                                                         ?? double.PositiveInfinity)}\n");
	}
}