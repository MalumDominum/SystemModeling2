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
double modelingTime = 300;

#region CP-1 Task 1: Just one Device

//var c1 = new CreateDevice("Create 1", () => 0.25);
//var p1 = new ProcessDevice("Process 1", () => 1);
//c1.PathGroup = new() { Paths = { new(p1) } };

//model = new Model { Devices = { c1, p1 } };
//modelingTime = 20;

#endregion

#region CP-1 Task 3: Create with 3 serial Processes next

//var d1 = new CreateDevice("Create 1", () => 1);
//var d2 = new ProcessDevice("Process 1", () => 1);
//var d3 = new ProcessDevice("Process 2", () => 1);
//var d4 = new ProcessDevice("Process 3", () => 1);

//d1.PathGroup = new() { Paths = { new(d2) } };
//d2.PathGroup = new() { Paths = { new(d3) } };
//d3.PathGroup = new() { Paths = { new(d4) } };

//model = new Model { Devices = { d1, d2, d3, d4 } };
//modelingTime = 10;

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
//var process = new ProcessDevice("Process 1", () => 1, maxQueue: 0, processorsCount: 4);

//create.PathGroup = new(){ Paths = { new(process) } };

//model = new Model { Devices = { create, process } };
//modelingTime = 5;

#endregion

#region CP-1 Task 6: Out Paths Demo

//var d1 = new CreateDevice("Create 1", () => 0.2);
//var d11 = new ProcessDevice("Process 1 1", () => 1);
//var d12 = new ProcessDevice("Process 1 2", () => 1);
//var d21 = new ProcessDevice("Process 2 1", () => 1);
//var d22 = new ProcessDevice("Process 2 2", () => 1);

//d1.PathGroup = new(SelectionPath.Random) { Paths = { new(d11, 0.2), new(d21, 0.8) } };
//d11.PathGroup = new() { Paths = { new(d12) } };
//d21.PathGroup = new() { Paths = { new(d22) } };

//model = new Model { Devices = { d1, d11, d12, d21, d22 } };
//modelingTime = 50;

#endregion

#region CP-1 Task 6: Paths that Loops Demo

//var d1 = new CreateDevice("Create 1", () => 0.5);
//var d2 = new ProcessDevice("Process 1", () => 1, 5);
//var d3 = new ProcessDevice("Process 2", () => 2);

//d1.PathGroup = new() { Paths = { new(d2) } };
//d2.PathGroup = new() { Paths = { new(d3) } };
//d3.PathGroup = new() { Paths = { new(d2) } };

//model = new Model { Devices = { d1, d2, d3 } };
//modelingTime = 100;

#endregion

#region CP-2 Task 1: Paths Priority Demo

//var c1 = new CreateDevice("Create 1", () => 0.2);
//var p1 = new ProcessDevice("Process 1", () => 1, 3);
//var p2 = new ProcessDevice("Process 2", () => 1);
//var p3 = new ProcessDevice("Process 3", () => 1, 6);

//c1.PathGroup = new() { Paths = { new(p1, 3), new(p2, 2), new(p3) } };

//model = new Model { Devices = { c1, p1, p2, p3 } };
//modelingTime = 50;

#endregion

#region CP-2 Task 2: Banks

var dc1 = new CreateDevice("Create 1", RE.GetExponential(0.3), firstCreatingTime: 0.1);
var dp1 = new ProcessDevice("Process 1", RE.GetNormal(1, 0.3), 3, startedQueue: new[] { 1, 1, 1 });
var dp2 = new ProcessDevice("Process 2", RE.GetNormal(1, 0.3), 3, startedQueue: new[] { 1, 1, 1 });

dc1.PathGroup = new() { Paths = { new(dp1), new(dp2, 2) } };

dp1.MigrateOptions = new List<MigrateOption> { new(dp2, 2) };
dp2.MigrateOptions = new List<MigrateOption> { new(dp1, 2) };

model = new Model { Devices = { dc1, dp1, dp2 } };
//modelingTime = 300;

#endregion

#region CP-2 Task 3: The Hospital

//var dc1 = new CreateDevice("Create 1", () => RE.Exponential(15) * 0.5);         // RE.Exponential(15) * 0.5 + RE.Exponential(15) 
//var dc2 = new CreateDevice("Create 2", () => RE.Exponential(15) * 0.1, 2);      // RE.Exponential(15) * 0.1 + RE.Exponential(40)
//var dc3 = new CreateDevice("Create 3", () => RE.Exponential(15) * 0.4, 3);      // RE.Exponential(15) * 0.4 + RE.Exponential(30)

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
modelingTime = 10000;

#endregion

model.Simulate(modelingTime);