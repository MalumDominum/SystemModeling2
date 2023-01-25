namespace SystemModeling2.TacticalExperimenter;

public class ParameterDiffResults
{
    public string ParameterName { get; set; }

    public List<double> Results { get; set; } = new();

    public ParameterDiffResults(string parameterName) => ParameterName = parameterName;
}