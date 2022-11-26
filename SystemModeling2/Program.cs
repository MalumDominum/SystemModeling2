using SystemModeling2.Devices;
using SystemModeling2.Model;
using Path = SystemModeling2.Devices.Models.Path;
using RE = SystemModeling2.Infrastructure.RandomExtended;

//var d1 = new CreateDevice("Create 1", RE.GetUniform(1.5, 4));
//var d2 = new ProcessDevice("Process 1", RE.GetExponential(2), 3);
//var d3 = new ProcessDevice("Process 2", RE.GetUniform(1.5, 4));
//var d4 = new ProcessDevice("Process 3", RE.GetGaussian(3, 5), 2);

var d1 = new CreateDevice("Create 1", () => 0.25);
var d11 = new ProcessDevice("Process 1 1", () => 1, 5, 4);
var d12 = new ProcessDevice("Process 1 2", () => 1);
var d21 = new ProcessDevice("Process 2 1", () => 1);
var d22 = new ProcessDevice("Process 2 2", () => 1, 5);
d1.Paths = new List<Path> { new(d11), new(d21, 2) };
d11.Paths = new List<Path> { new(d12) };
d21.Paths = new List<Path> { new(d22) };
//var model = new Model { Devices = { d1, d11, d12, d21, d22 } };

//var d1 = new CreateDevice("Create 1", () => 10);
//var d2 = new ProcessDevice("Process 1", () => 1, 5);
//var d3 = new ProcessDevice("Process 2", () => 2);
//d1.Paths = new List<Path> { new(d2) };
//d2.Paths = new List<Path> { new(d3) };
//d3.Paths = new List<Path> { new(d2) };
//var model = new Model { Devices = { d1, d2, d3 } };


#region CP-2 Task 2 with Banks

//var dc1 = new CreateDevice("Create 1", RE.GetExponential(0.3), conditions: new StartedConditions(null, null, 0.1));
//var dp1 = new ProcessDevice("Process 1", RE.GetGaussian(1, 0.3), 3, conditions: new StartedConditions(1, new[] { 2 }, null));
//var dp2 = new ProcessDevice("Process 2", RE.GetGaussian(1, 0.3), 3, conditions: new StartedConditions(1, new[] { 2 }, null));
//
//dc1.Paths = new List<Path> { new(dp1), new(dp2, 2) };
//dp1.MigrateOptions = new List<ProcessDevice> { dp2 };
//dp2.MigrateOptions = new List<ProcessDevice> { dp1 };
//var model = new Model { Devices = { dc1, dp1, dp2 } };

#endregion

#region CP-2 Task 3 with the Hospital

var dc1 = new CreateDevice("Create 1", () => RE.Exponential(5) / 0.5);         // RE.Exponential(15) / 0.5 + RE.Exponential(15) 
var dc2 = new CreateDevice("Create 2", () => RE.Exponential(5) / 0.1, 2);	   // RE.Exponential(15) / 0.1 + RE.Exponential(40)
var dc3 = new CreateDevice("Create 3", () => RE.Exponential(5) / 0.4, 3);      // RE.Exponential(15) / 0.4 + RE.Exponential(30)

var doctors = new ProcessDevice("Doctors", RE.GetExponential(15), -1, 2, new List<int> { 1 });
var register = new ProcessDevice("Register", () => RE.Uniform(2, 5) + RE.Erlang(4.5, 3));
var laboratory = new ProcessDevice("Laboratory", () => RE.Erlang(4, 2) + RE.Uniform(2, 5));
var ward = new ProcessDevice("Ward", RE.GetUniform(3, 8), processorsCount: 3);

dc1.Paths = new List<Path> { new(doctors) };
dc2.Paths = new List<Path> { new(doctors) };
dc3.Paths = new List<Path> { new(doctors) };

doctors.Paths = new List<Path> { new(ward, 2, new List<int> { 1 }), new(register, 1, new List<int>{ 2, 3 }) };

register.Paths = new List<Path> { new(laboratory) };
laboratory.Paths = new List<Path> { new(ward, passTypes: new List<int> { 2 }) }; // Don't pass 3 type


var model = new Model { Devices = { dc1, dc2, dc3, doctors, ward, register, laboratory } };

#endregion

model.Simulate(10000);