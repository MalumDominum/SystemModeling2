using SystemModeling1;
using SystemModeling1.Devices;
using SystemModeling1.Model;

var d1 = new CreateDevice("Create 1", RandomExtended.GetGaussian(3, 2));
var d2 = new ProcessDevice("Process 1", RandomExtended.GetExponential(2), 3);
var d3 = new ProcessDevice("Process 2", RandomExtended.GetUniform(1, 5));
var d4 = new ProcessDevice("Process 3", RandomExtended.GetGaussian(4, 2));

d1.NextPriorityTuples = new List<(Device, int)> { (d2, 1) };
d2.NextPriorityTuples = new List<(Device, int)> { (d3, 1) };
d3.NextPriorityTuples = new List<(Device, int)> { (d4, 1) };

var model = new Model { Devices = { d1, d2, d3, d4 } };
model.Simulate(1000);