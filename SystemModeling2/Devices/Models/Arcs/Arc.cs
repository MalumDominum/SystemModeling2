using SystemModeling2.Devices.Interfaces;

namespace SystemModeling2.Devices.Models.Arcs;

public class Arc<TSource, TDestination> : IPrioritized
    where TSource : IDevice
    where TDestination : IDevice
{
    #region Properties

    public TSource Source { get; set; }

    public TDestination Destination { get; set; }

    public int Multiplicity { get; set; }

    public int Priority { get; set; } // PassProbability?

    //public List<int>? PassTypes { get; set; }

    #endregion

    #region Constructor

    public Arc(TSource source, TDestination destination, int multiplicity = 1, int priority = 1)
    {
        Source = source;
        Destination = destination;
        Multiplicity = multiplicity;
        Priority = priority;
        //PassTypes = passTypes;
    }

    #endregion
}