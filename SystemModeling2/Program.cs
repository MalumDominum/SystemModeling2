using SystemModeling2.Devices;
using SystemModeling2.Model;
using RE = SystemModeling2.RandomExtended;

//var d1 = new CreateDevice("Create 1", RE.GetUniform(1.5, 4));
//var d2 = new ProcessDevice("Process 1", RE.GetExponential(2), 3);
//var d3 = new ProcessDevice("Process 2", RE.GetUniform(1.5, 4));
//var d4 = new ProcessDevice("Process 3", RE.GetGaussian(3, 5), 2);

//var d1 = new CreateDevice("Create 1", () => 0.25);
//var d11 = new ProcessDevice("Process 1 1", () => 1, 5);
//var d12 = new ProcessDevice("Process 1 2", () => 1);
//var d21 = new ProcessDevice("Process 2 1", () => 1);
//var d22 = new ProcessDevice("Process 2 2", () => 1, 5);

// CP-2 Task 2 with Banks
var dc1 = new CreateDevice("Create 1", RE.GetExponential(0.5));
var dp1 = new ProcessDevice("Process 1", RE.GetExponential(0.3), 3);
var dp2 = new ProcessDevice("Process 2", RE.GetExponential(0.3), 3);

dc1.NextPriorityTuples = new List<(Device, int)> { (dp1, 2), (dp2, 1) };

var model = new Model { Devices = { dc1, dp1, dp2 } };
model.Simulate(1000);