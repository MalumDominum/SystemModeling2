using Distributions = MathNet.Numerics.Distributions;

namespace SystemModeling2.Infrastructure;

public class RandomExtended
{
    private static readonly Random Rnd = new();

    #region Curried Functions

    public static Func<double> GetExponential(double meanValue) =>
        () => Exponential(meanValue);

    public static Func<double> GetUniform(double minValue, double maxValue) =>
        () => Uniform(Math.Min(minValue, maxValue), Math.Max(minValue, maxValue));

    public static Func<double> GetNormal(double meanValue, double meanDeviationValue) =>
        () => Normal(meanValue, meanDeviationValue);

    public static Func<double> GetErlang(double rateLambda, int shapeK) =>
        () => Erlang(rateLambda, shapeK);

    #endregion

    public static double Exponential(double meanValue) => -meanValue * Math.Log(GetRandomNumber());

    public static double Uniform(double minValue, double timeMax) => minValue + GetRandomNumber() * (timeMax - minValue);

    public static double Normal(double meanValue, double meanDeviationValue) => Distributions.Normal.Sample(meanValue, meanDeviationValue);

    public static double Erlang(double rateLambda, int shapeK) => Distributions.Erlang.Sample(shapeK, rateLambda);

	// public static double Erlang(double rateLambda, int shapeK)
	//   {
	//       var accumulator = 0.0;
	//       if (shapeK < 1) throw new ArgumentException("Shape (k) value must be positive");
	//       if (rateLambda < 0) throw new ArgumentException("Rate (λ) value must be not negative");
	//       for (var i = 1; i < shapeK; i++)
	//           accumulator += Math.Log(GetRandomNumber());

	//       return -1 / rateLambda * accumulator;
	//   }

	private static double GetRandomNumber() => 1.0 - Rnd.NextDouble();
}