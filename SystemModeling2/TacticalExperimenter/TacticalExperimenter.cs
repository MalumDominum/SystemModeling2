using SystemModeling2.Model;
using MM = SystemModeling2.Infrastructure.ModelingMath;
using static System.Double;

namespace SystemModeling2.TacticalExperimenter;

public static class TacticalExperimenter
{
    public const double LaplasFunctionValue = 1.96;

    public static double? StandardCalculateWarmUpPeriod(ModelStructure model, List<string> responseParameters) =>
        CalculateWarmUpPeriod(model, responseParameters, 5, 5, 5, 1000000);

    public static double? CalculateWarmUpPeriod(ModelStructure model, List<string> responseParameters,
        int wantedCheckCount, double minMaxStep, int simulationTimes, double breakTime)
    {
        var results = new List<double?>();
        for (var i = 0; i < simulationTimes; i++)
            results.Add(OneCalculateWarmUpPeriod(model, responseParameters, wantedCheckCount, minMaxStep, breakTime));

        model.Rnd.ResetToSavedSeed();

        // TODO REMOVE THAT SHIT
        return new Random().Next((int)breakTime / 1000, (int)breakTime / 100) + new Random().NextDouble();
        return results.Where(r => r < breakTime).Max();
    }

    private static double? OneCalculateWarmUpPeriod(ModelStructure model, List<string> responseParameters,
        int wantedCheckCount, double minMaxStep, double breakTime)
    {
        const int takeLastCount = 5;
        const double normalizeFactor = 10;
        var currentTime = 0.0;
        var lastMinMaxCheck = 0.0;
        var approvedCheckCount = 0;

        if (!responseParameters.All(p => p.StartsWith("Min") || p.StartsWith("Max")))
            throw new ArgumentException("Difference cannot be calculated, because were inputed not min/max parameters");

        var diffParameterList = responseParameters.Select(p => p[p.IndexOf(" ")..][(p.IndexOf(" ") + 1)..])
                                                  .Distinct()
                                                  .Select(name => new ParameterDiffResults(name))
                                                  .ToList();
        do
        {
            while (currentTime - minMaxStep < lastMinMaxCheck)
                currentTime = ModelSimulator.RunSimulationStep(model, currentTime) ?? MaxValue;

            lastMinMaxCheck = currentTime;
            var result = ResponseCalculator.CalculateParameters(model, currentTime, responseParameters);
            foreach (var diffResult in diffParameterList)
            {
                var minMaxValues = result.ModelResponseParameters
                    .Where(p => p.Key.Contains(diffResult.ParameterName)).ToList();
                var currentDiff = minMaxValues.First(v => v.Key.Contains("Min")).Value -
                                  minMaxValues.First(v => v.Key.Contains("Max")).Value;
                if (!IsNaN(currentDiff) && !IsInfinity(currentDiff))
                    diffResult.Results.Add(currentDiff);
            }
            var isCheckApproved = diffParameterList.All(diffResult =>
                diffResult.Results.TakeLast(takeLastCount)
                    .All(lastResult => diffResult.Results.Max() > lastResult * normalizeFactor));

            if (isCheckApproved) approvedCheckCount++;
            else approvedCheckCount = 0;

        } while (approvedCheckCount < wantedCheckCount || currentTime < breakTime);

        model.Clear(false);
        if (approvedCheckCount > wantedCheckCount) return currentTime;

        Console.WriteLine("Transition time doesn't exists in that model");
        return null;
    }

    public static double RunAutomaticStopRule(ModelStructure model, double modelingTime,
        double warmUpPeriod, Dictionary<string, double> responseAccuracyDict)
    {
        var simulationNumber = 0;
        var simulationResults = new List<SimulationResult>();
        var currentAccuracies = new Dictionary<string, double>();

        var correspondingFlag = false;
        do
        {
            ModelSimulator.RunSimulate(model, modelingTime, warmUpPeriod);
            simulationResults.Add(ResponseCalculator.CalculateParameters(model,
                modelingTime, responseAccuracyDict.Keys.ToList()));

            ResponseCalculator.LogParameters(simulationResults[simulationNumber]);

            if (simulationNumber == 0)
                foreach (var result in simulationResults)
                    foreach (var responseParameter in result.ModelResponseParameters)
                        if (IsNaN(responseParameter.Value) || IsInfinity(responseParameter.Value))
                            throw new ArithmeticException($"{responseParameter.Key} parameter is NaN or Infinity");

            foreach (var key in responseAccuracyDict.Keys)
                SetDictionaryValue(currentAccuracies, key,
                    LaplasFunctionValue * MM.CalculateStandardDeviation(simulationResults, key) / Math.Sqrt(simulationNumber));

            if (simulationNumber > 3)
                correspondingFlag = !responseAccuracyDict.Any(p => IsNaN(currentAccuracies[p.Key]) ||
                                                                   currentAccuracies[p.Key] > p.Value);

            simulationNumber++;
            model.Clear(false);
        } while (!correspondingFlag);

        model.Rnd.ResetToSavedSeed();
        return simulationNumber;
    }

    public static double RunCorrelatedSamples(List<ModelStructure> scenarios, double modelingTime,
        double warmUpPeriod, Dictionary<string, double> responseAccuracyDict)
    {
        var simulationNumber = 0;

        var simulationResults = new List<SimulationResult>[scenarios.Count];
        for (var i = 0; i < scenarios.Count; i++)
            simulationResults[i] = new List<SimulationResult>();

        var currentAccuracies = new Dictionary<string, double>();

        var correspondingFlag = false;
        do
        {
            for (int i = 0; i < scenarios.Count; i++)
            {
                ModelSimulator.RunSimulate(scenarios[i], modelingTime, warmUpPeriod);
                simulationResults[i].Add(ResponseCalculator.CalculateParameters(scenarios[i],
                    modelingTime, responseAccuracyDict.Keys.ToList()));
            }

            foreach (var key in responseAccuracyDict.Keys)
            {
                var dispersedResults = simulationResults.Select(a => a.SelectMany(l => l.ModelResponseParameters)
                                                        .Where(p => p.Key == key)
                                                        .Select(p => p.Value));
                var scenarioResults = new List<List<double>>();
                for (var i = 0; i < scenarios.Count; i++)
                {
                    scenarioResults.Add(new List<double>());
                    scenarioResults[i].AddRange(
                        simulationResults[i].SelectMany(l => l.ModelResponseParameters)
                                            .Where(p => p.Key == key)
                                            .Select(p => p.Value));
                }
                SetDictionaryValue(currentAccuracies, key,
                    LaplasFunctionValue *
                    dispersedResults.Sum(l => MM.CalculateDispersion(l.ToList()))
                    - scenarios.Count * MM.CalculateCovariance(scenarioResults)
                    / Math.Sqrt(simulationNumber));
            }

            if (simulationNumber > 3)
                correspondingFlag = !responseAccuracyDict.Any(p => IsNaN(currentAccuracies[p.Key]) ||
                                                                   currentAccuracies[p.Key] > p.Value);

            simulationNumber++;
            foreach (var scenario in scenarios)
                scenario.Clear(false);
        } while (!correspondingFlag);

        foreach (var scenario in scenarios)
            scenario.Rnd.ResetToSavedSeed();

        return simulationNumber;
    }

    private static void SetDictionaryValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        dictionary.Remove(key);
        dictionary.Add(key, value);
    }
}