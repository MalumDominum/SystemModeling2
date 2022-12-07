using SystemModeling2.Devices.Models;

namespace SystemModeling2.Infrastructure;

public static class ToStringConvertor
{
	public static string StringifyList(IEnumerable<double> list) =>
		list.Select(x => x != double.MaxValue ? $"{Math.Round(x, 5)}" : "MAX")
			.Aggregate((a, c) => $"{a}, {c}");

	public static string StringifyList(IEnumerable<int> list) => list.Select(x => $"{x}").Aggregate((a, c) => $"{a}, {c}");

	public static string StringifyDict(IDictionary<int, double> dictionary, double? modelingTime = null) =>
		dictionary.Any()
			? dictionary.OrderBy(d => d.Key)
			      .Select(d => $"{d.Key} = {Math.Round((double)(modelingTime != null ? d.Value / modelingTime : d.Value), 6)}")
				  .Aggregate((a, c) => $"{a}, {c}")
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
}