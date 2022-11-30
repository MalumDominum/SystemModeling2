using SystemModeling2.Devices;
using SystemModeling2.Devices.Models.Arcs;

namespace SystemModeling2.Infrastructure;

public static class ArcBuilder
{
    public static OutputArc Build(Transition source, Place destination,
        int multiplicity = 1, int priority = 1)
    {
        var arc = new OutputArc(source, destination, multiplicity, priority);

        source.OutArcs.Add(arc);

        if (destination.InArcs == null)
            destination.InArcs = new List<OutputArc> { arc };
        else destination.InArcs.Add(arc);

        return arc;
    }

    public static InputArc Build(Place source, Transition destination,
        int multiplicity = 1, int priority = 1, bool isInformational = false)
    {
        var arc = new InputArc(source, destination, multiplicity, priority, isInformational);

        if (source.OutArcs == null)
            source.OutArcs = new List<InputArc> { arc };
        else source.OutArcs.Add(arc);

        destination.InArcs.Add(arc);

        return arc;
    }
}