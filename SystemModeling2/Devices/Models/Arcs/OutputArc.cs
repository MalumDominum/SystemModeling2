namespace SystemModeling2.Devices.Models.Arcs;

public class OutputArc : Arc<Transition, Place>
{
    public OutputArc(Transition source, Place destination, int multiplicity = 1, int priority = 1)
        : base(source, destination, multiplicity, priority) { }
}