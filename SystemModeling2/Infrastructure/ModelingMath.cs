using SystemModeling2.Model;

namespace SystemModeling2.Infrastructure;

public static class ModelingMath
{
    public static double CalculateStandardDeviation(List<SimulationResult> simulationResults, string parameterKey) =>
        CalculateStandardDeviation(simulationResults.Select(r => r.ModelResponseParameters[parameterKey]).ToList());

    public static double CalculateCovariance(List<SimulationResult> simulationResults) =>
        CalculateCovariance(simulationResults.Select(r => r.ModelResponseParameters.Values.ToList()).ToList());

    public static double CalculateStandardDeviation(List<double> responseParameters) =>
        Math.Sqrt(CalculateDispersion(responseParameters));

    public static double CalculateDispersion(List<double> responseParameters)
    {
        var avgResponse = responseParameters.Average();
        return responseParameters.Select(p => Math.Pow(p - avgResponse, 2)).Average();
    }

    public static double CalculateCovariance(List<List<double>> responseParametersByScenario)
    {
        var count = responseParametersByScenario.Count;

        var avgByScenario = responseParametersByScenario.Select(parameters => parameters.Average()).ToList();
        var scenariosProducts = new List<double>();
        for (var i = 0; i < count; i++)
            scenariosProducts.AddRange(responseParametersByScenario[i]
                .Select(parameter => parameter - avgByScenario[i]));
        var result = scenariosProducts.Sum() / (count - 1);
        return result;
    }
}