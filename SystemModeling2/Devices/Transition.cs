using SystemModeling2.Devices.Interfaces;
using SystemModeling2.Devices.Models;
using SystemModeling2.Devices.Models.Arcs;

namespace SystemModeling2.Devices;

public class Transition : IDevice, IPrioritized
{
	#region Logic Properties

	public string Name { get; set; }

	// TODO public int ChannelsCount { get; set; }

	public double NextTime { get; } // TODO NextTimes[]

	public Func<double> DistributionFunc { get; init; }

	public List<InputArc> InArcs { get; set; }

	public List<OutputArc> OutArcs { get; set; }

	public int Priority { get; set; }

	public bool MayFire => InArcs.TrueForAll(a => a.Source.Tokens.Count >= a.Multiplicity);

	#endregion

	#region Statistics Properties
	
	public int Passed { get; set; }

	public int Outputted { get; set; }

	#endregion

	#region Constructor

	public Transition(string name, Func<double> distributionFunc, int priority = 1)
	{
		Name = name;
		NextTime = double.MaxValue;
		DistributionFunc = distributionFunc;
		Priority = priority;

		InArcs = new List<InputArc>();
		OutArcs = new List<OutputArc>();
	}

	#endregion

	public void Fire()
	{
		var passedTokens = new List<Token>();
		foreach (var arc in InArcs)
		{
			for (var i = 0; i < arc.Multiplicity; i++)
			{
				passedTokens.Add(arc.Source.Tokens.Dequeue());
				Passed++;
			}
		}
		//passedTokens = passedTokens.Distinct().ToList();
		foreach (var arc in OutArcs)
		{
			for (var i = 0; i < arc.Multiplicity; i++)
			{
				arc.Destination.Tokens.Enqueue(passedTokens[i]);
				Outputted++;
			}
		}
		Console.WriteLine($"{this} fired");
	}

	public override string ToString() => $"{Name}";
}