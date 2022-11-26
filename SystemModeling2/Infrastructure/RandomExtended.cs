namespace SystemModeling2.Infrastructure;

public class RandomExtended
{
    private static readonly Random Rnd = new();

    #region CurriedFunctions

    public static Func<double> GetExponential(double meanValue) =>
        () => Exponential(meanValue);

    public static Func<double> GetUniform(double minValue, double maxValue) =>
        () => Uniform(Math.Min(minValue, maxValue), Math.Max(minValue, maxValue));

    public static Func<double> GetGaussian(double meanValue, double meanDeviationValue) =>
        () => Gaussian(meanValue, meanDeviationValue);

    public static Func<double> GetErlang(double rateLambda, int shapeK) =>
        () => Erlang(rateLambda, shapeK);

    #endregion

    public static double Exponential(double meanValue) => -meanValue * Math.Log(GetRandomNumber());

    public static double Uniform(double minValue, double timeMax) => minValue + GetRandomNumber() * (timeMax - minValue);

    public static double Gaussian(double meanValue, double meanDeviationValue) =>
        meanValue + meanDeviationValue *
        (Math.Sqrt(-2.0 * Math.Log(GetRandomNumber())) *
         Math.Sin(2.0 * Math.PI * GetRandomNumber()));

    public static double Erlang(double rateLambda, int shapeK)
    {
        var accumulator = 0.0;
        if (shapeK < 1) throw new ArgumentException("Shape (K) value must be positive");
        for (var i = 1; i < shapeK; i++)
            accumulator += Math.Log(GetRandomNumber());

        return -1 / rateLambda * accumulator;
    }

    private static double GetRandomNumber() => 1.0 - Rnd.NextDouble();
}