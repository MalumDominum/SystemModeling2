using System.Text;
using SystemModeling2.Infrastructure;
using SystemModeling2.Model;
using SystemModeling2.TacticalExperimenter;
using RE = SystemModeling2.Infrastructure.RandomExtended;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
ColoredConsole.OutputTurnedOff = true;

var rnd = new Random();
var sharedSeed = rnd.Next();

var rnd1 = new RE(sharedSeed);
var scenario1 = ModelsAccessible.ThreeSerialProcesses(RE.GetExponential(60),
    RE.GetNormal(70, 10), RE.GetUniform(50, 120), RE.GetExponential(80), rnd1);

var rnd2 = new RE(sharedSeed);
var scenario2 = ModelsAccessible.ThreeSerialProcesses(RE.GetExponential(90),
    RE.GetNormal(70, 10), RE.GetUniform(50, 120), RE.GetExponential(80), rnd2);

var rnd3 = new RE(sharedSeed);
var scenario3 = ModelsAccessible.ThreeSerialProcesses(RE.GetExponential(80),
    RE.GetNormal(70, 10), RE.GetUniform(50, 120), RE.GetExponential(80), rnd3);

var scenarios = new List<ModelStructure> { scenario1, scenario2, scenario3 };

var warmUpPeriodNullable = TacticalExperimenter.CalculateWarmUpPeriod(scenario1, new List<string>
    { "Min of MeanProcessingTime", "Max of MeanProcessingTime", "Min of MeanLoad", "Max of MeanLoad" },
    5, 50, 10, 100000);

if (!warmUpPeriodNullable.HasValue)
    throw new ArithmeticException("Wamp-Up Period does not exists for this model");
var warmUpPeriod = warmUpPeriodNullable.Value;
var modelingTime = warmUpPeriod * 10;

var numberOfSimulation = TacticalExperimenter.RunAutomaticStopRule(scenario1,
    modelingTime, warmUpPeriod, new Dictionary<string, double>
    {
        { "Min of MeanProcessingTime", 0.2 },
        { "Max of MeanProcessingTime", 0.2 },
        { "Min of MeanLoad", 0.2 },
        { "Max of MeanLoad", 0.2 }
    });

Console.WriteLine($@"Result of tactical planning at one scenario:
    Warm-Up Period - {warmUpPeriod}; Modeling Time - {modelingTime}
    Number of Simulations - {numberOfSimulation}");
ModelSimulator.RunExperiment(new List<ModelStructure> { scenario1 }, modelingTime, numberOfSimulation, warmUpPeriod);

Console.WriteLine("\n===================Run Comparative Experiment===================\n");

var experimentNumberOfSimulation = TacticalExperimenter.RunCorrelatedSamples(scenarios,
    modelingTime, warmUpPeriod, new Dictionary<string, double>
    {
        { "Min of MeanProcessingTime", 0.2 },
        { "Max of MeanProcessingTime", 0.2 },
        { "Min of MeanLoad", 0.2 },
        { "Max of MeanLoad", 0.2 }
    });
Console.WriteLine($@"Result of running Correlated Samples method:
    Number of Simulations - {experimentNumberOfSimulation}");
ModelSimulator.RunExperiment(scenarios, modelingTime, experimentNumberOfSimulation, warmUpPeriod);