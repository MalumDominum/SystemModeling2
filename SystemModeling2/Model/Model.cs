using SystemModeling2.Devices;

namespace SystemModeling2.Model;

public class Model
{
    public List<CreateDevice> CreateDevices { get; }

    public List<ProcessDevice> ProcessDevices { get; }

    public List<Device> Devices { get; }

    public Model(List<CreateDevice> createDevices, List<ProcessDevice> processDevices)
    {
        CreateDevices = createDevices;
        ProcessDevices = processDevices;
        Devices = new (createDevices.Union<Device>(processDevices));
    }
}