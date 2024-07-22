namespace SzkolenieAI.Helpers;

internal class CostCalculator(double inputPrice, double outputPrice)
{
    private const double PlnToUsd = 3.93;
    private const int Divider = 1000000;

    public double CalculateInputCost(int inputTokens)
    {
        return inputTokens * inputPrice / Divider;
    }

    public double CalculateOutputCost(int outputTokens)
    {
        return outputTokens * outputPrice / Divider;
    }

    public double CalculateTotalCost(int inputTokens, int outputTokens)
    {
        return CalculateInputCost(inputTokens) + CalculateOutputCost(outputTokens);
    }

    public double CalculateTotalCostInPln(int inputTokens, int outputTokens)
    {
        return CalculateInputCost(inputTokens) + CalculateOutputCost(outputTokens) * PlnToUsd;
    }

    public void WriteCosts(int inputTokens, int outputTokens)
    {
        ColoredConsole.WriteSeparator("\n\n");
        ColoredConsole.WriteInfo("COSTS:");
        ColoredConsole.WriteInfo($"Input tokens tokens: {inputTokens} = {CalculateInputCost(inputTokens)} $");
        ColoredConsole.WriteInfo($"Output tokens: {outputTokens} = {CalculateOutputCost(outputTokens)} $");
        ColoredConsole.WriteInfo($"Total cost: {CalculateTotalCost(inputTokens, outputTokens)} zł");
        ColoredConsole.WriteSeparator(suffix: "\n");
    }
}