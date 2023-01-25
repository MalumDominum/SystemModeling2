using Distributions = MathNet.Numerics.Distributions;

namespace SystemModeling2.Infrastructure;

public class RandomExtended
{
    private readonly int? _savedSeed;

    private Random Rnd { get; set; }

    public RandomExtended(int? seed = null)
    {
        _savedSeed = seed;
        Rnd = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public void ResetToSavedSeed() => Rnd = _savedSeed.HasValue ? new Random(_savedSeed.Value) : new Random();

    public void SetSeed(int seed) => Rnd = new Random(seed);

    #region Curried Functions

    public static Func<RandomExtended?, double> GetExponential(double meanValue) =>
        rnd => rnd!.Exponential(meanValue);

    public static Func<RandomExtended?, double> GetUniform(double minValue, double maxValue) =>
        rnd => rnd!.Uniform(Math.Min(minValue, maxValue), Math.Max(minValue, maxValue));

    public static Func<RandomExtended?, double> GetNormal(double meanValue, double meanDeviationValue) =>
        rnd => rnd!.Normal(meanValue, meanDeviationValue);

    public static Func<RandomExtended?, double> GetErlang(double rateLambda, int shapeK) =>
        rnd => rnd!.Erlang(rateLambda, shapeK);

    #endregion

    #region Distribution Functions
    
    public double Exponential(double meanValue) => -meanValue * Math.Log(GetRandomNumber());

    public double Uniform(double minValue, double timeMax) => minValue + GetRandomNumber() * (timeMax - minValue);

    public double Normal(double meanValue, double meanDeviationValue) => Distributions.Normal.Sample(meanValue, meanDeviationValue);

    public double Erlang(double rateLambda, int shapeK) => Distributions.Erlang.Sample(shapeK, rateLambda);
    
	private double GetRandomNumber() => 1.0 - Rnd.NextDouble();

    #endregion
}