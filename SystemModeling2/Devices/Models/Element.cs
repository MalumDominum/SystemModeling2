namespace SystemModeling2.Devices.Models;

public class Element
{
	public int Type { get; set; }

	public double CreatingTime { get; set; }

	public double? OutOfSystemTime { get; set; }

	public double? LiveTime => OutOfSystemTime != null ? OutOfSystemTime - CreatingTime : null;

	public Element(int type, double creatingTime)
	{
		Type = type;
		CreatingTime = creatingTime;
	}
}