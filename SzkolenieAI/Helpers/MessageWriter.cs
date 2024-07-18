namespace SzkolenieAI.Helpers;

internal static class MessageWriter
{
    public static void PrintWelcomeMessage()
    {
        const string message =
            "Witaj w wersji demonstracyjnej ChatGPT OpenAI API!\n"
            + "Możesz rozpocząć czat z asystentem AI.\n"
            + "Wpisz 'exit', aby zakończyć sesję czatu.\n";
        ColoredConsole.WriteSystem(message);
    }

    public static void WriteCosts(int inputTokens, int outputTokens)
    {
        WriteSeparator("\n\n");
        ColoredConsole.WriteInfo("COSTS:");
        ColoredConsole.WriteInfo($"Input tokens tokens: {inputTokens} = {CostCalculator.CalculateInputCost(inputTokens)} $");
        ColoredConsole.WriteInfo($"Output tokens: {outputTokens} = {CostCalculator.CalculateOutputCost(outputTokens)} $");
        ColoredConsole.WriteInfo($"Total cost: {CostCalculator.CalculateTotalCost(inputTokens, outputTokens)} zł");
        WriteSeparator(suffix: "\n");
    }

    public static void WriteExitMessage()
    {
        const string message = "Dziękujemy za skorzystanie z sesji czatu. Do zobaczenia!";
        ColoredConsole.WriteSystem(message);
    }

    public static void WriteSeparator(string prefix = "", string suffix = "")
    {
        const string message = "--------------------------------------------------";
        ColoredConsole.WriteInfo(prefix + message + suffix);
    }
}