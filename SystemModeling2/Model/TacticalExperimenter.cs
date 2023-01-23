namespace SystemModeling2.Model;

public static class TacticalExperimenter
{
    public static double? CalculateTransitionTime()
    {
        var result = 100;

        if (result != null) return result;

        Console.WriteLine("Transition time doesn't exists in that model");
        return null;
    }
 
    public static (double, int) RunTacticalPlanning(Model model, double wantedConfidenceLevel, double wantedAccuracy)
    {
        var modelingTime = 0;
        var numberOfSimulations = 0;
        return (numberOfSimulations, modelingTime);
    }

    public static (double, int) RunTacticalPlanning(List<Model> scenarios, double wantedConfidenceLevel, double wantedAccuracy)
    {
        var modelingTime = 0;
        var numberOfSimulations = 0;
        return (numberOfSimulations, modelingTime);
    }
}