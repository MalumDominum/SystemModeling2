namespace SystemModeling2;

public static class ColoredConsole
{
    public static bool OutputTurnedOff { get; set; }

	public static void WriteLine(string text, ConsoleColor color)
	{
		if (OutputTurnedOff) return;

		var previousColor = Console.ForegroundColor;
		Console.ForegroundColor = color;
		Console.WriteLine(text);
		Console.ForegroundColor = previousColor;
	}
}