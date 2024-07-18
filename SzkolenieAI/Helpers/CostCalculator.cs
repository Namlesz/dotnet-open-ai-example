namespace SzkolenieAI.Helpers;

internal static class CostCalculator
{
    private const int Dividor = 1000000;
    private const double PlnToUsd = 3.93;
    public static double CalculateInputCost(int inputTokens)
    {
        return inputTokens * 0.5/Dividor;
    }

    public static double CalculateOutputCost(int outputTokens)
    {
        return outputTokens * 1.5/Dividor;
    }

    public static double CalculateTotalCost(int inputTokens, int outputTokens)
    {
        return CalculateInputCost(inputTokens) + CalculateOutputCost(outputTokens);
    }

    public static double CalculateTotalCostInPln(int inputTokens, int outputTokens)
    {
        return CalculateInputCost(inputTokens) + CalculateOutputCost(outputTokens) * PlnToUsd;
    }
}