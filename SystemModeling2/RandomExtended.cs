namespace SystemModeling2;

public class RandomExtended
{
	private static readonly Random Rnd = new();

	#region CurriedFunctions

	public static Func<double> GetExponential(double meanTime) => () => Exponential(meanTime);

	public static Func<double> GetUniform(double minTime, double timeMax) => () => Uniform(minTime, timeMax);

	public static Func<double> GetGaussian(double meanTime, double meanDeviationTime) => () => Gaussian(meanTime, meanDeviationTime);

	#endregion

	public static double Exponential(double meanTime) => -meanTime * Math.Log(GetRandomNumber());

	public static double Uniform(double minTime, double timeMax) => minTime + GetRandomNumber() * (timeMax - minTime);

	public static double Gaussian(double meanTime, double meanDeviationTime) =>
		meanTime + meanDeviationTime *
		(Math.Sqrt(-2.0 * Math.Log(GetRandomNumber())) *
		 Math.Sin(2.0 * Math.PI * GetRandomNumber()));

	private static double GetRandomNumber() => 1.0 - Rnd.NextDouble();
}