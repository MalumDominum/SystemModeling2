using System.Text;
using SystemModeling2.Devices;
using SystemModeling2.Devices.Enums;
using SystemModeling2.Devices.Models;
using SystemModeling2.Model;
using Path = SystemModeling2.Devices.Models.Path;
using RE = SystemModeling2.Infrastructure.RandomExtended;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Model model;

#region CP-1 Task 1: Just one Device

//var c1 = new CreateDevice("Create 1", () => 0.25);
//var p1 = new ProcessDevice("Process 1", () => 1);
//c1.PathGroup = new() { Paths = { new(p1) } };

//model = new Model { Devices = { c1, p1 } };

#endregion

#region CP-1 Task 3: Create with 3 serial Processes next

//var d1 = new CreateDevice("Create 1", RE.GetUniform(2, 4));
//var d2 = new ProcessDevice("Process 1", RE.GetExponential(0.5));
//var d3 = new ProcessDevice("Process 2", RE.GetUniform(2, 4));
//var d4 = new ProcessDevice("Process 3", RE.GetUniform(3, 5));

//d1.PathGroup = new() { Paths = { new(d2) } };
//d2.PathGroup = new() { Paths = { new(d3) } };
//d3.PathGroup = new() { Paths = { new(d4) } };

//model = new Model { Devices = { d1, d2, d3, d4 } };

#endregion

#region CP-1 Task 3: Verification of Logic

//var d1 = new CreateDevice("Create 1", () => 0.2);
//var d2 = new ProcessDevice("Process 1", () => 1);
//var d3 = new ProcessDevice("Process 2", () => 1);
//var d4 = new ProcessDevice("Process 3", () => 1);

//d1.PathGroup = new() { Paths = { new(d2) } };
//d2.PathGroup = new() { Paths = { new(d3) } };
//d3.PathGroup = new() { Paths = { new(d4) } };

//model = new Model { Devices = { d1, d2, d3, d4 } };

#endregion

#region CP-1 Task 5: Many Processors Demo

//var create = new CreateDevice("Create 1", () => 0.2);
//var process = new ProcessDevice("Process 1", () => 1, maxQueue: 2, processorsCount: 4);

//create.PathGroup = new(){ Paths = { new(process) } };

//model = new Model { Devices = { create, process } };

#endregion

#region CP-1 Task 6: Out Paths Demo

//var d1 = new CreateDevice("Create 1", () => 0.2);
//var d11 = new ProcessDevice("Process 1 1", () => 1);
//var d12 = new ProcessDevice("Process 1 2", () => 1);
//var d21 = new ProcessDevice("Process 2 1", () => 1);
//var d22 = new ProcessDevice("Process 2 2", () => 1);

//d1.PathGroup = new(SelectionPath.Random) { Paths = { new(d11, 0.5), new(d21, 0.5) } };
//d11.PathGroup = new() { Paths = { new(d12) } };
//d21.PathGroup = new() { Paths = { new(d22) } };

//model = new Model { Devices = { d1, d11, d12, d21, d22 } };

#endregion

#region CP-1 Task 6: Paths that Loops Demo

//var d1 = new CreateDevice("Create 1", () => 10);
//var d2 = new ProcessDevice("Process 1", () => 1, 5);
//var d3 = new ProcessDevice("Process 2", () => 2);

//d1.PathGroup = new() { Paths = { new(d2) } };
//d2.PathGroup = new() { Paths = { new(d3) } };
//d3.PathGroup = new() { Paths = { new(d2) } };

//model = new Model { Devices = { d1, d2, d3 } };

#endregion


#region CP-2 Task 2: Banks

var dc1 = new CreateDevice("Create 1", RE.GetExponential(0.3), firstCreatingTime: 0.1);
var dp1 = new ProcessDevice("Process 1", RE.GetNormal(1, 0.3), 3, startedQueue: new[] { 1, 1, 1 });
var dp2 = new ProcessDevice("Process 2", RE.GetNormal(1, 0.3), 3, startedQueue: new[] { 1, 1, 1 });

dc1.PathGroup = new() { Paths = { new(dp1), new(dp2, 2) } };

dp1.MigrateOptions = new List<ProcessDevice> { dp2 };
dp2.MigrateOptions = new List<ProcessDevice> { dp1 };

model = new Model { Devices = { dc1, dp1, dp2 } };

#endregion

#region CP-2 Task 3: The Hospital

//var dc1 = new CreateDevice("Create 1", () => RE.Exponential(5) * 0.5);         // RE.Exponential(15) * 0.5 + RE.Exponential(15) 
//var dc2 = new CreateDevice("Create 2", () => RE.Exponential(5) * 0.1, 2);      // RE.Exponential(15) * 0.1 + RE.Exponential(40)
//var dc3 = new CreateDevice("Create 3", () => RE.Exponential(5) * 0.4, 3);      // RE.Exponential(15) * 0.4 + RE.Exponential(30)

//var doctors = new ProcessDevice("Doctors", RE.GetExponential(15), int.MaxValue, 2, new List<int> { 1 });
//var register = new ProcessDevice("Register", () => RE.Uniform(2, 5) + RE.Erlang(4.5, 3));
//var laboratory = new ProcessDevice("Laboratory", () => RE.Erlang(4, 2) + RE.Uniform(2, 5));
//var ward = new ProcessDevice("Ward", RE.GetUniform(3, 8), processorsCount: 3);

//var toDoctors = new PathGroup { Paths = { new(doctors) } };
//dc1.PathGroup = toDoctors;
//dc2.PathGroup = toDoctors;
//dc3.PathGroup = toDoctors;

//doctors.PathGroup = new() { Paths = { new(ward, 2, new List<int> { 1 }), new(register, 1, new List<int> { 2, 3 }) } };

//register.PathGroup = new() { Paths = { new(laboratory) } };
//laboratory.PathGroup = new() { Paths = { new(ward, passTypes: new List<int> { 2 }) } }; // Don't pass 3 type


//model = new Model { Devices = { dc1, dc2, dc3, doctors, ward, register, laboratory } };

#endregion

model.Simulate(500);