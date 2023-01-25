namespace SystemModeling2.Model;

public class SimulationResult
{
    public Dictionary<string, double> ModelResponseParameters { get; } = new();

    public ModelStructure Model { get; }

    public int? SimulationNumber { get; }

    public SimulationResult(ModelStructure model, int? simulationNumber = null)
    {
        Model = model;
        SimulationNumber = simulationNumber;
    }
}