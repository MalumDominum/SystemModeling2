namespace SystemModeling2.Infrastructure;

public static class ToStringConvertor
{
	public static string StringifyList(IEnumerable<double> list) =>
		list.Select(x => x != double.MaxValue ? $"{Math.Round(x, 5)}" : "MAX")
			.Aggregate((a, c) => $"{a}, {c}");

	public static string StringifyList(IEnumerable<int> list) => list.Select(x => $"{x}").Aggregate((a, c) => $"{a}, {c}");

	public static string StringifyDict(IDictionary<int, double> dictionary, double? modelingTime = null) =>
		dictionary.OrderBy(d => d.Key)
			      .Select(d => $"{d.Key} = {(modelingTime != null ? d.Value / modelingTime : d.Value)}")
				  .Aggregate((a, c) => $"{a}, {c}");
}