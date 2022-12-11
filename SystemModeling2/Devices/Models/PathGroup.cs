using SystemModeling2.Devices.Enums;

namespace SystemModeling2.Devices.Models;

public class PathGroup
{
	public SelectionPath SelectionPath { get; set; }

	public List<Path> Paths { get; set; }

	public PathGroup(SelectionPath selectionPath = SelectionPath.Priority)
	{
		SelectionPath = selectionPath;
		Paths = new List<Path>();
	}
}