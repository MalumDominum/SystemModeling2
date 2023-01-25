namespace SystemModeling2.TacticalExperimenter;

public class ExperimenterResult
{
    public double Result { get; }

    public double ReceivedAccuracy { get; }

    public Dictionary<string, double> ModelResponseParameters { get; set; }

    public ExperimenterResult(double result, double receivedAccuracy,
        Dictionary<string, double> modelResponseParameters)
    {
        Result = result;
        ReceivedAccuracy = receivedAccuracy;
        ModelResponseParameters = modelResponseParameters;
    }
}