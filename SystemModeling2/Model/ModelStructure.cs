using SystemModeling2.Devices;
using SystemModeling2.Infrastructure;

namespace SystemModeling2.Model;

public class ModelStructure
{
    public string? Name { get; set; }

    public List<CreateDevice> CreateDevices { get; }

    public List<ProcessDevice> ProcessDevices { get; }

    public List<Device> Devices { get; }

    public RandomExtended Rnd { get; }

    public List<Func<RandomExtended?, double>> DistributionFuncs => Devices.Select(d => d.DistributionFunc).ToList();
    
    public ModelStructure(List<CreateDevice> createDevices, List<ProcessDevice> processDevices, RandomExtended? rnd = null)
    {
        Rnd = rnd ?? new RandomExtended();
        CreateDevices = createDevices;
        ProcessDevices = processDevices;
        Devices = new(createDevices.Union<Device>(processDevices));
    }

    public void Clear(bool resetToSavedSeed, int? newSeed = null)
    {
        var rnd = new Random();
        if (resetToSavedSeed)
            Rnd.ResetToSavedSeed();
        else Rnd.SetSeed(newSeed ?? rnd.Next());

        CreateDevice.AllElements.Clear();
        foreach (var device in Devices) device.Reset();
    }
}