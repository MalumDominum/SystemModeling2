using SystemModeling2.Devices;
using SystemModeling2.Devices.Interfaces;
using SC = SystemModeling2.Infrastructure.ToStringConvertor;

namespace SystemModeling2;

public class Model
{
    #region Properties

    public List<Place> Places { get; init; }

    public List<Transition> Transitions { get; init; }

    #endregion

    #region Constructor

    public Model()
    {
        Places = new List<Place>();
        Transitions = new List<Transition>();
    }

    #endregion

    public void Simulate(double modelingTime)
    {
        var currentTime = 0.0;

        while (currentTime < modelingTime)
        {
            var activeTransitions = Transitions.Where(t => t.MayFire).ToList();
            var nextTime = activeTransitions.Min(activeT => activeT.NextTime);

            if (nextTime > modelingTime) break;
            currentTime = nextTime;

            var conflictedTransitions = activeTransitions.Where(t => t.NextTime == nextTime).ToList();
            var firedTransition = GetRandomByPriority(conflictedTransitions);

            firedTransition.Fire();
        }
        ShowStatistics(modelingTime);
    }

    public static T GetRandomByPriority<T>(List<T> conflictedElements) where T : class, IPrioritized
    {
        var prioritySum = conflictedElements.Sum(t => t.Priority);
        var randomValue = new Random().Next(1, prioritySum);
        T? result = null;
        foreach (var t in conflictedElements)
        {
            randomValue -= t.Priority;
            if (randomValue > 0) continue;

            result = t;
            break;
        }
        return result!;
    }

    public static void ShowStatistics(double modelingTime)
    {
        /*var createPlaces = GetSpecificPlaces<CreateDevice>(devices);
	    var processPlaces = GetSpecificPlaces<ProcessDevice>(devices);
		
		Console.WriteLine("\n");
		var createdSum = createPlaces.Sum(d => d.Finished);
		foreach (var device in createPlaces)
		    Console.WriteLine($"Device {device.Name} Created: {device.Finished} of type {device.CreatingType}");
	    Console.WriteLine($"Sum of Created: {createdSum}\n");
		
		foreach (var device in processPlaces)
		    Console.WriteLine($"Device {device.Name} Processed: {SC.StringifyTypesCount(device.Processed)}, Sum: {device.Finished}");
		Console.WriteLine($"Sum of Processed: {processPlaces.Sum(d => d.Finished)}\n");

		var rejectedSum = processPlaces.Sum(d => d.Rejected);
		if (processPlaces.Any(d => d.Rejected > 0))
		{
			foreach (var device in processPlaces)
				Console.WriteLine($"Device {device.Name} Rejected: {device.Rejected}");
			Console.WriteLine($"Sum of Rejected: {rejectedSum} / {Math.Round((double)rejectedSum / createdSum * 100, 2)}%\n");
	    }
	    else ColoredConsole.WriteLine("Places doesn't reject elements\n", ConsoleColor.DarkGray);

		if (processPlaces.Any(d => d.Migrated > 0))
		{
			foreach (var device in processPlaces)
				Console.WriteLine($"Device {device.Name} Migrated:\t{device.Migrated}");
			Console.WriteLine($"Sum of Migrated: {processPlaces.Sum(d => d.Migrated)}\n");
		}
		else ColoredConsole.WriteLine("Elements doesn't migrated between devices\n", ConsoleColor.DarkGray);

		foreach (var device in processPlaces)
			Console.WriteLine($"Device {device.Name} MeanBusyTime: {device.MeanBusyTime}");
		Console.WriteLine($"MeanBusyTime of all: {processPlaces.Select(d => d.MeanBusyTime).Average()} with {modelingTime} total\n");
		
		foreach (var device in processPlaces)
			Console.WriteLine($"Device {device.Name} MeanInQueue: {device.MeanInQueue / modelingTime}");
		Console.WriteLine($"MeanInQueue of all: {processPlaces.Select(d => d.MeanInQueue / modelingTime).Average()}\n");

		foreach (var device in processPlaces)
			Console.WriteLine($"Device {device.Name} MeanIncomingTime: {SC.StringifyDict(device.IncomingTimes, modelingTime)}");
		Console.WriteLine($"MeanIncomingTime of all: {processPlaces.Average(d => d.IncomingTimes.Average(x => x.Value / modelingTime))}\n");

		foreach (var type in CreateDevice.AllElements.Select(e => e.Type).Distinct().Order())
			Console.WriteLine($"Element type {type}: MeanLiveTime - " +
			                  $"{CreateDevice.AllElements.Where(e => e.Type == type).Average(e => e.LiveTime)}");
		Console.WriteLine($"MeanLiveTime of all: {CreateDevice.AllElements.Select(e => e.LiveTime).Average()}\n");
		*/
    }

    /*private static List<T> GetSpecificPlaces<T>(List<Device> devices) where T : Device
    {
	    var specificPlaces = new List<T>();
	    foreach (var d in devices)
		    if (d is T cd)
			    specificPlaces.Add(cd);
	    return specificPlaces;
	}*/
}