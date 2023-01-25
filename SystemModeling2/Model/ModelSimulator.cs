using SystemModeling2.Devices;
using SystemModeling2.Infrastructure;
using static System.Double;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;

namespace SystemModeling2.Model;

public static class ModelSimulator
{
    public static double? RunSimulationStep(ModelStructure model, double currentTime, double? modelingTime = null, bool? warmUpPeriodGone = null)
    {
        var nextTime = model.Devices.Min(d => d.NextTimes.Min());
        if (nextTime > (modelingTime ?? MaxValue)) return null;
        var nextDevices = model.Devices.Where(d => d.NextTimes.Contains(nextTime));

		if (warmUpPeriodGone ?? true)
            foreach (var device in model.Devices)
                device.StatisticsCollectionDisabled = false;

        foreach (var device in model.ProcessDevices.Where(d => !d.StatisticsCollectionDisabled))
            device.DoStatistics(Math.Min(nextTime, modelingTime ?? MaxValue) - currentTime);

        currentTime = nextTime;

        foreach (var device in nextDevices)
            device.OutAction(currentTime);

        foreach (var device in model.Devices)
            if (device is ProcessDevice processDevice)
                processDevice.TryMigrate(currentTime);

		return nextTime;
    }

    public static void RunSimulate(ModelStructure model, double modelingTime, double warmUpPeriod = 0)
	{
		var currentTime = 0.0;
		if (warmUpPeriod > 0)
            foreach (var device in model.Devices)
                device.StatisticsCollectionDisabled = true;
		
        while (currentTime < modelingTime)
            currentTime = RunSimulationStep(model, currentTime, modelingTime, currentTime > warmUpPeriod)
                          ?? MaxValue;

        //ShowStatistics(model.CreateDevices, model.ProcessDevices, modelingTime);
    }
	
    public static List<SimulationResult> RunExperiment(List<ModelStructure> models, double modelingTime,
        double numberOfSimulations, double warmUpPeriod = 0)
    {
		var rnd = new Random();
        var seedSequence = new List<int>();
        for (int i = 0; i < numberOfSimulations; i++)
            seedSequence.Add(rnd.Next());

		var results = new List<SimulationResult>();

        foreach (var model in models)
        {
            for (int currentSimulation = 0; currentSimulation < numberOfSimulations; currentSimulation++)
            {
                RunSimulate(model, modelingTime, warmUpPeriod);
                results.Add(ResponseCalculator.CalculateParameters(model, modelingTime, null, currentSimulation));
                model.Clear(false, seedSequence[currentSimulation]);
            }
			model.Rnd.ResetToSavedSeed();
        }
        return results;
    }

    private static double Round(double value, int digits = 5) => Math.Round(value, digits);

    public static void ShowStatistics(List<CreateDevice> createDevices,
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
		                      $"MeanProcessingTime: {Round(device.BusyTime / device.Finished)}");
		Console.WriteLine($"Avg of Processed: {processDevices.Average(d => d.Finished)}, " +
		                  $"Avg of MeanProcessingTime: {Round(processDevices.Average(d => d.BusyTime / d.Finished))}\n");

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
			                                ?? PositiveInfinity)}");
		Console.WriteLine($"Average of MeanLiveTime: {Round(CreateDevice.AllElements.Select(e => e.LiveTime).Average()
		                                                         ?? PositiveInfinity)}\n");
	}
}