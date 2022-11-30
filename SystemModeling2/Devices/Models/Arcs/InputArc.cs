namespace SystemModeling2.Devices.Models.Arcs;

public class InputArc : Arc<Place, Transition>
{
    public bool IsInformational { get; set; } // TODO Check for another non-informational arc

    public InputArc(Place source, Transition destination, int multiplicity = 1, int priority = 1, bool isInformational = false)
        : base(source, destination, multiplicity, priority)
    {
        IsInformational = isInformational;
    }
}