using SystemModeling2.Devices;
using SystemModeling2.Model;
using RE = SystemModeling2.RandomExtended;

var d1 = new CreateDevice("Create 1", RE.GetUniform(1.5, 4));
var d2 = new ProcessDevice("Process 1", RE.GetExponential(2), 3);
var d3 = new ProcessDevice("Process 2", RE.GetUniform(1.5, 4));
var d4 = new ProcessDevice("Process 3", RE.GetGaussian(3, 5), 2);

//var d1 = new CreateDevice("Create 1", () => 1);
//var d2 = new ProcessDevice("Process 1", () => 1, 3);
//var d3 = new ProcessDevice("Process 2", () => 1);
//var d4 = new ProcessDevice("Process 3", () => 1, 2);

d1.NextPriorityTuples = new List<(Device, int)> { (d2, 1) };
d2.NextPriorityTuples = new List<(Device, int)> { (d3, 1) };
d3.NextPriorityTuples = new List<(Device, int)> { (d4, 1) };

var model = new Model { Devices = { d1, d2, d3, d4 } };
model.Simulate(1000);