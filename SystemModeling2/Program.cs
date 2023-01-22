using System.Text;
using SystemModeling2.Model;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var model = ModelsAccessible.Banks();
double modelingTime = 300;
ModelSimulator.Simulate(model, modelingTime);