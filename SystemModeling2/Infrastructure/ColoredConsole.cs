namespace SystemModeling2;

public static class ColoredConsole
{
	public static void WriteLine(string text, ConsoleColor color)
	{
		var previousColor = Console.ForegroundColor;
		Console.ForegroundColor = color;
		Console.WriteLine(text);
		Console.ForegroundColor = previousColor;
	}
}