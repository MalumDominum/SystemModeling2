using SystemModeling2.Devices;
using static System.String;

namespace SystemModeling2.Model;

public static class ResponseCalculator
{
    public static SimulationResult CalculateParameters(ModelStructure model, double modelingTime,
        List<string>? responseParameters = null, int? simulationNumber = null)
    {
        var result = new SimulationResult(model, simulationNumber);

        var createdSum = model.CreateDevices.Sum(d => d.Finished);
        var rejectedSum = model.ProcessDevices.Sum(d => d.Rejected);
        var meanLoadValues = model.ProcessDevices.Select(d => d.BusyTime / modelingTime);
        AddResponseParameters(result, new Dictionary<string, double>
        {
            { "Sum of Created", createdSum },
            { "Sum of Processed", model.ProcessDevices.Sum(d => d.Finished) },
            { "Avg of Processed", model.ProcessDevices.Average(d => d.Finished) },
            { "Min of MeanProcessingTime", model.ProcessDevices.Min(d => d.BusyTime / d.Finished) },
            { "Avg of MeanProcessingTime", model.ProcessDevices.Average(d => d.BusyTime / d.Finished) },
            { "Max of MeanProcessingTime", model.ProcessDevices.Max(d => d.BusyTime / d.Finished) },
            { "Sum of Rejected", rejectedSum },
            { "Rejecting Chance", createdSum != 0 ? rejectedSum / createdSum * 100 : double.PositiveInfinity },
            { "Sum of Migrated", model.ProcessDevices.Sum(d => d.Migrated) },
            { "Min of MeanLoad", meanLoadValues.Min() },
            { "Avg of MeanLoad", meanLoadValues.Average() },
            { "Max of MeanLoad", meanLoadValues.Max() },
            { "Avg of MeanInQueue", model.ProcessDevices.Select(d => d.MeanInQueue / modelingTime).Average() },
            { "Avg of MeanIncomingInterval", model.ProcessDevices.Average(d => d.IncomingDeltas.Sum(x => x.Value.Average())) },
            { "Avg of ElementsLiveTime", CreateDevice.AllElements.Select(e => e.LiveTime).Average() ?? double.PositiveInfinity }
        }, responseParameters);

        return result;
    }

    public static void LogParameters(SimulationResult result)
    {
        Console.WriteLine((!IsNullOrEmpty(result.Model.Name) ? $"\nModel {result.Model.Name}" : "\nModel") +
            (result.SimulationNumber.HasValue ? $" with Simulation number {result.SimulationNumber} parameters:\n\t" : "parameters:\n\t") +
            result.ModelResponseParameters.Select(p => $"{p.Key}: {p.Value}").Aggregate((a, c) => $"{a}\n\t{c}"));
    }

    private static void AddResponseParameters(SimulationResult result, Dictionary<string, double> responseDict, List<string>? responseParameters = null)
    {
        foreach (var response in responseDict)
            AddResponseParameter(result, response.Key, response.Value, responseParameters);
    }

    private static void AddResponseParameter(SimulationResult result, string key, double value,
                                             List<string>? responseParameters = null)
    {
        if (responseParameters == null || responseParameters.Contains(key))
            result.ModelResponseParameters.Add(key, value);
    }
}