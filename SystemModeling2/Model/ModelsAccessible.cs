using SystemModeling2.Devices;
using SystemModeling2.Devices.Enums;
using SystemModeling2.Devices.Models;
using RE = SystemModeling2.Infrastructure.RandomExtended;

namespace SystemModeling2.Model;

public static class ModelsAccessible
{
    #region JustOneDevice

    public static ModelStructure JustOneDevice(Func<RE?, double> createInterval, Func<RE?, double> processInterval, RE? rnd = null)
    {
        var c1 = new CreateDevice("Create 1", createInterval);
        var p1 = new ProcessDevice("Process 1", processInterval);
        c1.PathGroup = new() { Paths = { new(p1) } };

        return new ModelStructure(new List<CreateDevice> { c1 },
                                  new List<ProcessDevice> { p1 }, rnd);
    }

    #endregion

    #region ThreeSerialProcesses

    public static ModelStructure ThreeSerialProcesses(Func<RE?, double> createInterval, Func<RE?, double> process1Interval,
                                             Func<RE?, double> process2Interval, Func<RE?, double> process3Interval, RE? rnd = null)
    {
        var d1 = new CreateDevice("Create 1", createInterval, rnd);
        var d2 = new ProcessDevice("Process 1", process1Interval, rnd);
        var d3 = new ProcessDevice("Process 2", process2Interval, rnd);
        var d4 = new ProcessDevice("Process 3", process3Interval, rnd);

        d1.PathGroup = new() { Paths = { new(d2) } };
        d2.PathGroup = new() { Paths = { new(d3) } };
        d3.PathGroup = new() { Paths = { new(d4) } };

        return new ModelStructure(new List<CreateDevice> { d1 },
                                  new List<ProcessDevice> { d2, d3, d4 }, rnd);
    }

    #endregion

    #region ManyProcessorsDemo

    public static ModelStructure ManyProcessorsDemo()
    {
        var create = new CreateDevice("Create 1", _ => 0.2);
        var process = new ProcessDevice("Process 1", _ => 1, maxQueue: 2, processorsCount: 3);

        create.PathGroup = new() { Paths = { new(process) } };

        return new ModelStructure(new List<CreateDevice> { create },
                                  new List<ProcessDevice> { process });
    }

    #endregion

    #region OutPathsDemo

    public static ModelStructure OutPathsDemo()
    {
        var d1 = new CreateDevice("Create 1", _ => 0.2);
        var d11 = new ProcessDevice("Process 1 1", _ => 1);
        var d12 = new ProcessDevice("Process 1 2", _ => 1);
        var d21 = new ProcessDevice("Process 2 1", _ => 1);
        var d22 = new ProcessDevice("Process 2 2", _ => 1);

        d1.PathGroup = new(SelectionPath.Random) { Paths = { new(d11, 0.02), new(d21, 0.98) } };
        d11.PathGroup = new() { Paths = { new(d12) } };
        d21.PathGroup = new() { Paths = { new(d22) } };

        return new ModelStructure(new List<CreateDevice> { d1 },
                         new List<ProcessDevice> { d11, d12, d21, d22 });
    }

    #endregion

    #region PathsThatLoopsDemo

    public static ModelStructure PathsThatLoopsDemo()
    {
        var d1 = new CreateDevice("Create 1", _ => 0.5);
        var d2 = new ProcessDevice("Process 1", _ => 1, maxQueue: 5);
        var d3 = new ProcessDevice("Process 2", _ => 2);

        d1.PathGroup = new() { Paths = { new(d2) } };
        d2.PathGroup = new() { Paths = { new(d3) } };
        d3.PathGroup = new() { Paths = { new(d2) } };

        return new ModelStructure(new List<CreateDevice> { d1 },
                         new List<ProcessDevice> { d2, d3 });
    }

    #endregion

    #region PathsPriorityDemo

    public static ModelStructure PathsPriorityDemo()
    {
        var c1 = new CreateDevice("Create 1", _ => 0.2);
        var p1 = new ProcessDevice("Process 1", _ => 1, maxQueue: 3);
        var p2 = new ProcessDevice("Process 2", _ => 1);
        var p3 = new ProcessDevice("Process 3", _ => 1, maxQueue: 6);

        c1.PathGroup = new() { Paths = { new(p1, 3), new(p2, 2), new(p3) } };

        return new ModelStructure(new List<CreateDevice> { c1 },
                         new List<ProcessDevice> { p1, p2, p3 });
    }

    #endregion

    #region Banks

    public static ModelStructure Banks()
    {
        var random = new RE();
        var dc1 = new CreateDevice("Create 1", RE.GetExponential(0.5), random, firstCreatingTime: 0.1);
        var dp1 = new ProcessDevice("Process 1", RE.GetNormal(1, 0.3), random, 3, startedQueue: new[] { 1, 1, 1 });
        var dp2 = new ProcessDevice("Process 2", RE.GetNormal(1, 0.3), random, 3, startedQueue: new[] { 1, 1, 1 });

        dc1.PathGroup = new(SelectionPath.UniformPriority) { Paths = { new(dp1), new(dp2, 2) } };

        dp1.MigrateOptions = new List<MigrateOption> { new(dp2, 2) };
        dp2.MigrateOptions = new List<MigrateOption> { new(dp1, 2) };

        return new ModelStructure(new List<CreateDevice> { dc1 },
                         new List<ProcessDevice> { dp1, dp2 });
    }
    
    #endregion

    #region Hospital

    public static ModelStructure Hospital()
    {
        var random = new RE();
        var dc1 = new CreateDevice("Create 1", rnd => rnd!.Exponential(15) * 0.5 + rnd.Exponential(15), random);         // rnd.Exponential(15) * 0.5 + rnd.Exponential(15) 
        var dc2 = new CreateDevice("Create 2", rnd => rnd!.Exponential(15) * 0.1 + rnd.Exponential(40), random, 2);      // rnd.Exponential(15) * 0.1 + rnd.Exponential(40)
        var dc3 = new CreateDevice("Create 3", rnd => rnd!.Exponential(15) * 0.4 + rnd.Exponential(30), random, 3);      // rnd.Exponential(15) * 0.4 + rnd.Exponential(30)

        var doctors = new ProcessDevice("Doctors", RE.GetExponential(15), random, int.MaxValue, 2, new List<int> { 1 });
        var register = new ProcessDevice("Register", rnd => rnd!.Uniform(2, 5) + rnd.Erlang(4.5, 3));
        var laboratory = new ProcessDevice("Laboratory", rnd => rnd!.Erlang(4, 2) + rnd.Uniform(2, 5));
        var ward = new ProcessDevice("Ward", RE.GetUniform(3, 8), random, processorsCount: 3);

        var toDoctors = new PathGroup { Paths = { new(doctors) } };
        dc1.PathGroup = toDoctors;
        dc2.PathGroup = toDoctors;
        dc3.PathGroup = toDoctors;

        doctors.PathGroup = new() { Paths = { new(ward, 1, new List<int> { 1 }), new(register, 2, new List<int> { 2, 3 }) } };

        register.PathGroup = new() { Paths = { new(laboratory) } };
        laboratory.PathGroup = new() { Paths = { new(ward, passTypes: new List<int> { 2 }) } }; // Don't pass 3 type


        return new ModelStructure(new List<CreateDevice> { dc1, dc2, dc3 },
                         new List<ProcessDevice> { doctors, ward, register, laboratory });
    }
    
    #endregion
}