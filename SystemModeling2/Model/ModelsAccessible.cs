using SystemModeling2.Devices;
using SystemModeling2.Devices.Enums;
using SystemModeling2.Devices.Models;
using RE = SystemModeling2.Infrastructure.RandomExtended;

namespace SystemModeling2.Model;

public static class ModelsAccessible
{
    #region JustOneDevice

    public static Model JustOneDevice(Func<double> createInterval, Func<double> processInterval)
    {
        var c1 = new CreateDevice("Create 1", () => 0.25);
        var p1 = new ProcessDevice("Process 1", () => 1);
        c1.PathGroup = new() { Paths = { new(p1) } };

        return new Model(new List<CreateDevice> { c1 },
                         new List<ProcessDevice> { p1 });
    }

    public static Func<Model> GetJustOneDevice(params Func<double>[] dFuncs) =>
        () => JustOneDevice(dFuncs[0], dFuncs[1]);

    #endregion

    #region ThreeSerialProcesses

    public static Model ThreeSerialProcesses()
    {
        var d1 = new CreateDevice("Create 1", () => 1);
        var d2 = new ProcessDevice("Process 1", () => 1);
        var d3 = new ProcessDevice("Process 2", () => 1);
        var d4 = new ProcessDevice("Process 3", () => 1);

        d1.PathGroup = new() { Paths = { new(d2) } };
        d2.PathGroup = new() { Paths = { new(d3) } };
        d3.PathGroup = new() { Paths = { new(d4) } };

        return new Model(new List<CreateDevice> { d1 },
                         new List<ProcessDevice> { d2, d3, d4 });
    }

    public static Func<Model> GetThreeSerialProcesses(params Func<double>[] dFuncs) =>
        () => ThreeSerialProcesses();

    #endregion

    #region ManyProcessorsDemo

    public static Model ManyProcessorsDemo()
    {
        var create = new CreateDevice("Create 1", () => 0.2);
        var process = new ProcessDevice("Process 1", () => 1, maxQueue: 2, processorsCount: 3);

        create.PathGroup = new() { Paths = { new(process) } };

        return new Model(new List<CreateDevice> { create },
                         new List<ProcessDevice> { process });
    }

    public static Func<Model> GetManyProcessorsDemo(params Func<double>[] dFuncs) =>
        () => ManyProcessorsDemo();

    #endregion

    #region OutPathsDemo

    public static Model OutPathsDemo()
    {
        var d1 = new CreateDevice("Create 1", () => 0.2);
        var d11 = new ProcessDevice("Process 1 1", () => 1);
        var d12 = new ProcessDevice("Process 1 2", () => 1);
        var d21 = new ProcessDevice("Process 2 1", () => 1);
        var d22 = new ProcessDevice("Process 2 2", () => 1);

        d1.PathGroup = new(SelectionPath.Random) { Paths = { new(d11, 0.02), new(d21, 0.98) } };
        d11.PathGroup = new() { Paths = { new(d12) } };
        d21.PathGroup = new() { Paths = { new(d22) } };

        return new Model(new List<CreateDevice> { d1 },
                         new List<ProcessDevice> { d11, d12, d21, d22 });
    }

    public static Func<Model> GetOutPathsDemo(params Func<double>[] dFuncs) =>
        () => OutPathsDemo();

    #endregion

    #region PathsThatLoopsDemo

    public static Model PathsThatLoopsDemo()
    {
        var d1 = new CreateDevice("Create 1", () => 0.5);
        var d2 = new ProcessDevice("Process 1", () => 1, 5);
        var d3 = new ProcessDevice("Process 2", () => 2);

        d1.PathGroup = new() { Paths = { new(d2) } };
        d2.PathGroup = new() { Paths = { new(d3) } };
        d3.PathGroup = new() { Paths = { new(d2) } };

        return new Model(new List<CreateDevice> { d1 },
                         new List<ProcessDevice> { d2, d3 });
    }

    public static Func<Model> GetPathsThatLoopsDemo(params Func<double>[] dFuncs) =>
        () => PathsThatLoopsDemo();

    #endregion

    #region PathsPriorityDemo

    public static Model PathsPriorityDemo()
    {
        var c1 = new CreateDevice("Create 1", () => 0.2);
        var p1 = new ProcessDevice("Process 1", () => 1, 3);
        var p2 = new ProcessDevice("Process 2", () => 1);
        var p3 = new ProcessDevice("Process 3", () => 1, 6);

        c1.PathGroup = new() { Paths = { new(p1, 3), new(p2, 2), new(p3) } };

        return new Model(new List<CreateDevice> { c1 },
                         new List<ProcessDevice> { p1, p2, p3 });
    }

    public static Func<Model> GetPathsPriorityDemo(params Func<double>[] dFuncs) =>
        () => PathsPriorityDemo();

    #endregion

    #region Banks

    public static Model Banks()
    {
        var dc1 = new CreateDevice("Create 1", RE.GetExponential(0.5), firstCreatingTime: 0.1);
        var dp1 = new ProcessDevice("Process 1", RE.GetNormal(1, 0.3), 3, startedQueue: new[] { 1, 1, 1 });
        var dp2 = new ProcessDevice("Process 2", RE.GetNormal(1, 0.3), 3, startedQueue: new[] { 1, 1, 1 });

        dc1.PathGroup = new(SelectionPath.UniformPriority) { Paths = { new(dp1), new(dp2, 2) } };

        dp1.MigrateOptions = new List<MigrateOption> { new(dp2, 2) };
        dp2.MigrateOptions = new List<MigrateOption> { new(dp1, 2) };

        return new Model(new List<CreateDevice> { dc1 },
                         new List<ProcessDevice> { dp1, dp2 });
    }

    public static Func<Model> GetBanks(params Func<double>[] dFuncs) =>
        () => Banks();

    #endregion

    #region Hospital

    public static Model Hospital()
    {
        var dc1 = new CreateDevice("Create 1", () => RE.Exponential(15) * 0.5 + RE.Exponential(15));         // RE.Exponential(15) * 0.5 + RE.Exponential(15) 
        var dc2 = new CreateDevice("Create 2", () => RE.Exponential(15) * 0.1 + RE.Exponential(40), 2);      // RE.Exponential(15) * 0.1 + RE.Exponential(40)
        var dc3 = new CreateDevice("Create 3", () => RE.Exponential(15) * 0.4 + RE.Exponential(30), 3);      // RE.Exponential(15) * 0.4 + RE.Exponential(30)

        var doctors = new ProcessDevice("Doctors", RE.GetExponential(15), int.MaxValue, 2, new List<int> { 1 });
        var register = new ProcessDevice("Register", () => RE.Uniform(2, 5) + RE.Erlang(4.5, 3));
        var laboratory = new ProcessDevice("Laboratory", () => RE.Erlang(4, 2) + RE.Uniform(2, 5));
        var ward = new ProcessDevice("Ward", RE.GetUniform(3, 8), processorsCount: 3);

        var toDoctors = new PathGroup { Paths = { new(doctors) } };
        dc1.PathGroup = toDoctors;
        dc2.PathGroup = toDoctors;
        dc3.PathGroup = toDoctors;

        doctors.PathGroup = new() { Paths = { new(ward, 1, new List<int> { 1 }), new(register, 2, new List<int> { 2, 3 }) } };

        register.PathGroup = new() { Paths = { new(laboratory) } };
        laboratory.PathGroup = new() { Paths = { new(ward, passTypes: new List<int> { 2 }) } }; // Don't pass 3 type


        return new Model(new List<CreateDevice> { dc1, dc2, dc3 },
                         new List<ProcessDevice> { doctors, ward, register, laboratory });
    }

    public static Func<Model> GetHospital(params Func<double>[] dFuncs) =>
        () => Hospital();

    #endregion
}