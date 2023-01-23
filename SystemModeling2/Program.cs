using System.Text;
using SystemModeling2;
using SystemModeling2.Model;
using RE = SystemModeling2.Infrastructure.RandomExtended;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
ColoredConsole.OutputTurnedOff = true;

const int sharedSeed = 1534371;

var rnd1 = new RE(sharedSeed);
var scenario1 = ModelsAccessible.ThreeSerialProcesses(RE.GetExponential(100),
    RE.GetNormal(70, 10), RE.GetUniform(50, 120), RE.GetExponential(80), rnd1);

var rnd2 = new RE(sharedSeed);
var scenario2 = ModelsAccessible.ThreeSerialProcesses(RE.GetExponential(90),
    RE.GetNormal(70, 10), RE.GetUniform(50, 120), RE.GetExponential(80), rnd2);

var rnd3 = new RE(sharedSeed);
var scenario3 = ModelsAccessible.ThreeSerialProcesses(RE.GetExponential(80),
    RE.GetNormal(70, 10), RE.GetUniform(50, 120), RE.GetExponential(80), rnd3);

var scenarios = new List<Model> { scenario1, scenario2, scenario3 };

var scenarioPlanningResults = TacticalExperimenter.RunTacticalPlanning(scenario1, 95, 0.01);
Console.WriteLine($@"Result of tactical planning at one scenario:
    Modeling time - {scenarioPlanningResults.Item1}
    Number of Simulations - {scenarioPlanningResults.Item2}");
ModelSimulator.RunExperiment(new List<Model> { scenario1 }, scenarioPlanningResults.Item1, scenarioPlanningResults.Item2);

Console.WriteLine("\n===================Run Comparative Experiment===================\n");

var experimentPlanningResults = TacticalExperimenter.RunTacticalPlanning(scenarios, 95, 0.01);
Console.WriteLine($@"Result of tactical planning comprassion many scenarios:
    Modeling time - {scenarioPlanningResults.Item1}
    Number of Simulations - {scenarioPlanningResults.Item2}");
ModelSimulator.RunExperiment(scenarios, experimentPlanningResults.Item1, experimentPlanningResults.Item2);