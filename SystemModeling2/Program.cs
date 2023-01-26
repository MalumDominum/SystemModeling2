using System.Text;
using SystemModeling2.Infrastructure;
using SystemModeling2.Model;
using SystemModeling2.TacticalExperimenter;
using static System.String;
using RE = SystemModeling2.Infrastructure.RandomExtended;

void Checkpoint(string name)
{
    Console.WriteLine($"======================Checkpoint {name}======================");
    Console.WriteLine("Please press any key to continue");
    Console.ReadKey();
}

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
ColoredConsole.OutputTurnedOff = true;

var rnd = new Random();
var sharedSeed = rnd.Next();

var rnd1 = new RE(sharedSeed);
var scenario1 = ModelsAccessible.RobotSystem(RE.GetExponential(40), 
    new List<(Func<RE?, double>?, Func<RE?, double>?)>
    {
        (RE.GetNormal(28, 2), RE.GetNormal(60, 10)),
        (RE.GetNormal(30, 2), RE.GetExponential(100)),
        (RE.GetNormal(26, 2), null)
    }, 3, rnd1);
scenario1.Name = "Standart scenario";

var rnd2 = new RE(sharedSeed);
var scenario2 = ModelsAccessible.RobotSystem(RE.GetExponential(40),
    new List<(Func<RE?, double>?, Func<RE?, double>?)>
    {
        (RE.GetNormal(50, 6), RE.GetNormal(60, 10)),
        (RE.GetNormal(30, 2), RE.GetExponential(150)),
        (RE.GetNormal(26, 2), null)
    }, 3, rnd2);
scenario2.Name = "Lower process ability scenario";

var scenarios = new List<ModelStructure> { scenario1, scenario2 };
/*
var nullableWarmUpPeriods = scenarios.Select(scenario =>
    TacticalExperimenter.CalculateWarmUpPeriod(scenario,
        new List<string>
            { "Min of MeanIncomingInterval", "Max of MeanIncomingInterval",
              "Min of MeanProcessingTime", "Max of MeanProcessingTime" },
        25, 100, 10, 100000)).ToList();

for (var i = 0; i < scenarios.Count; i++)
    if (!nullableWarmUpPeriods[i].HasValue)
        throw new ArithmeticException($"Wamp-Up Period does not exists for {scenarios[i].Name}");

Console.WriteLine($"Received results: {Join("", nullableWarmUpPeriods.Select(r => "\n" + r!.Value))}");
var warmUpPeriod = nullableWarmUpPeriods.Max(p => p!.Value);
var modelingTime = warmUpPeriod * 10;
Console.WriteLine($"\nWarm-up Period - {warmUpPeriod}; Modeling time - {modelingTime}\n");

Checkpoint("2: Run Correlated Samples Method");

var numberOfSimulation = TacticalExperimenter.RunAutomaticStopRule(scenario1,
    modelingTime, warmUpPeriod, new Dictionary<string, double>
    {
        { "Min of MeanIncomingInterval", 0.1 },
        { "Max of MeanIncomingInterval", 2 },
        { "Min of MeanProcessingTime", 0.1 },
        { "Max of MeanProcessingTime", 0.1 }
    });

Console.WriteLine($@"Result of tactical planning at first scenario:
    Warm-Up Period - {warmUpPeriod}; Modeling Time - {modelingTime}
    Number of Simulations - {numberOfSimulation}");

//ModelSimulator.RunExperiment(new List<ModelStructure> { scenario1 }, modelingTime, numberOfSimulation, warmUpPeriods);

Checkpoint("3: Run Comparative Experiment");

var experimentNumberOfSimulation = TacticalExperimenter.RunCorrelatedSamples(scenarios,
    modelingTime, warmUpPeriod, new Dictionary<string, double>
    {
        { "Min of MeanProcessingTime", 0.1 },
        { "Max of MeanProcessingTime", 0.1 },
        { "Min of MeanLoad", 0.1 },
        { "Max of MeanLoad", 0.1 }
    });
Console.WriteLine($@"Result of running Correlated Samples method:
    Number of Simulations - {experimentNumberOfSimulation}");

Checkpoint("3: Run Experiment");*/
var results = ModelSimulator.RunExperiment(scenarios, 32939.33964147102, 528, 3293.9339641471024);
var parameters = new List<string>
{
    "Min of MeanIncomingInterval", "Max of MeanIncomingInterval",
    "Min of MeanProcessingTime", "Max of MeanProcessingTime"
};
foreach (var parameter in parameters)
{
    var standardResult = results.Where(r => r.Model.Name == "Standart scenario").SelectMany(r => r.ModelResponseParameters.Where(p => p.Key == parameter)).Select(p => p.Value);
    var lowerResult = results.Where(r => r.Model.Name == "Lower process ability scenario").SelectMany(r => r.ModelResponseParameters.Where(p => p.Key == parameter)).Select(p => p.Value);

    Console.WriteLine("\nModel response parameter " + parameter + " results for Standart scenario:");
    Console.WriteLine("Min: " + standardResult.Min());
    Console.WriteLine("Avg: " + standardResult.Average());
    Console.WriteLine("Max: " + standardResult.Max() + "\n");

    Console.WriteLine("Model response parameter " + parameter + " results for Lower process ability scenario:");
    Console.WriteLine("Min: " + lowerResult.Min());
    Console.WriteLine("Avg: " + lowerResult.Average());
    Console.WriteLine("Max: " + lowerResult.Max() + "\n");
}