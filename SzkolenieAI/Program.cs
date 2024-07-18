using System.ClientModel;
using OpenAI.Chat;
using SzkolenieAI.Helpers;
using SzkolenieAI.Startup;

const string systemChatBehaviourDescription =
    "Jesteś wielkim pisarzem, Adamem Mickiewiczem. Wszystkie twoje odpowiedzi niech będą pisane wierszem.";

var configuration = SettingsConfiguration.BuildConfiguration();
var apiKey = configuration.GetOpenApiKey();
var engine = configuration.GetChatGptModel();

var client = new ChatClient(engine, apiKey);

var systemChatMessage = new SystemChatMessage(systemChatBehaviourDescription);

AsyncResultCollection<StreamingChatCompletionUpdate> updates
    = client.CompleteChatStreamingAsync("Kto był pierwszy, kura czy jajko?", systemChatMessage);

Console.WriteLine("[ASSISTANT]:");
await foreach (var update in updates)
{
    foreach (var updatePart in update.ContentUpdate)
    {
        Console.Write(updatePart.Text);
    }

    if (update is { Usage: not null })
    {
        var (inputTokens, outputTokens) = (update.Usage.InputTokens, update.Usage.OutputTokens);
        Console.WriteLine("\n\nCOSTS:");
        Console.WriteLine($"Input tokens tokens: {inputTokens} = {CostCalculator.CalculateInputCost(inputTokens)} $");
        Console.WriteLine($"Output tokens: {outputTokens} = {CostCalculator.CalculateOutputCost(outputTokens)} $");
        Console.WriteLine($"Total cost: {CostCalculator.CalculateTotalCost(inputTokens, outputTokens)} zł");
    }
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();