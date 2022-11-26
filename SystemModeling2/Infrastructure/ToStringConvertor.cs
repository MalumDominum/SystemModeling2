namespace SystemModeling2.Infrastructure;

public static class ToStringConvertor
{
	public static string StringifyList(IEnumerable<double> list) =>
		list.Select(x => x != double.MaxValue ? $"{Math.Round(x, 5)}" : "MAX")
			.Aggregate((a, c) => $"{a}, {c}");

	public static string StringifyList(IEnumerable<int> list) => list.Select(x => $"{x}").Aggregate((a, c) => $"{a}, {c}");

	public static string StringifyTypesCount(IReadOnlyCollection<int> elementTypes) =>
		elementTypes.Distinct()
			.OrderBy(t => t)
			.Aggregate("", (current, type) =>
				$"{current}{(current == "" ? "" : ", ")}" +
				$"{type} = {elementTypes.Count(t => t == type)}");
}