using SystemModeling2.Devices.Interfaces;
using SystemModeling2.Devices.Models;
using SystemModeling2.Devices.Models.Arcs;

namespace SystemModeling2.Devices;

public class Place : IDevice
{
	#region Properties

	public string Name { get; set; }

	public List<OutputArc>? InArcs { get; set; }

	public List<InputArc>? OutArcs { get; set; }

	public Queue<Token> Tokens { get; }

	#endregion

	#region Constructor

	public Place(string name, int initialTokensCount = 0)
	{
		Name = name;

		Tokens = new Queue<Token>();
		for (var i = 0; i < initialTokensCount; i++)
			Tokens.Enqueue(new Token());
	}

	#endregion
}