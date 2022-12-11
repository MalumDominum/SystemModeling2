using SystemModeling2.Devices.Models;

namespace SystemModeling2.Infrastructure;

public static class ToStringConvertor
{
	public static string StringifyList(IEnumerable<double> list) =>
		list.Select(x => x != double.MaxValue ? $"{Round(x)}" : "MAX")
			.Aggregate((a, c) => $"{a}, {c}");

	public static string StringifyDictOfLists(Dictionary<int, List<double>> dictionary) =>
		dictionary.Any()
			? dictionary.OrderBy(d => d.Key)
				  .Select(d => $"{d.Key} = {Round(d.Value.Average())}")
				  .Aggregate((a, c) => $"{a}, {c}") +
			  (dictionary.Keys.Count > 1 ? ", Sum: " + Round(dictionary.Values.Select(v => v.Average()).Sum()) : "")
			: "";

	public static string StringifyTypesCount(IReadOnlyCollection<Element> elements)
	{
		var result = elements.Select(e => e.Type)
			.Distinct()
			.Order()
			.ToList()
			.Aggregate("", (current, type) =>
				$"{current}{(current == "" ? "" : ", ")}" +
				$"{type} = {elements.Select(e => e.Type).Count(t => t == type)}");
		return result != "" ? result : "NULL";
	}

	public static string StringifyList(IEnumerable<int> list) => list.Select(x => $"{x}").Aggregate((a, c) => $"{a}, {c}");

	public static string StringifyDict(IDictionary<int, double> dictionary, double? divisor = null) =>
		dictionary.Any()
			? dictionary.OrderBy(d => d.Key)
			      .Select(d => $"{d.Key} = {Round(d.Value / (divisor ?? 1))}")
				  .Aggregate((a, c) => $"{a}, {c}") + 
				  ", Sum: " + Round(dictionary.Values.Select(v => v / (divisor ?? 1)).Sum())
			: "";

	private static double Round(double value) => Math.Round(value, 5);
}