using Microsoft.Extensions.DependencyInjection;
using SzkolenieAI;
using SzkolenieAI.Helpers;
using SzkolenieAI.Startup;

var configuration = SettingsConfiguration.BuildConfiguration();
var apiKey = configuration.GetOpenApiKey();
var engine = configuration.GetChatGptModel();
var inputTokenPrice = configuration.GetInputTokenCosts();
var outputTokenPrice = configuration.GetOutputTokenCosts();

var serviceProvider = new ServiceCollection()
    .AddSingleton(new CostCalculator(inputTokenPrice, outputTokenPrice))
    .BuildServiceProvider();

MessageWriter.PrintWelcomeMessage();

var chat = new Chat(engine, apiKey, serviceProvider.GetRequiredService<CostCalculator>());
while (true)
{
    var userInput = chat.GetUserInput();
    if (!userInput)
    {
        break;
    }

    await chat.ProcessChatSession();
}

ColoredConsole.WriteSystem($"Twoja podróż kosztowała: {chat.GetTotalCost()} zł");
MessageWriter.WriteExitMessage();
Console.ReadKey();