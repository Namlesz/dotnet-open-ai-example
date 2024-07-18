using SzkolenieAI;
using SzkolenieAI.Helpers;
using SzkolenieAI.Startup;

var configuration = SettingsConfiguration.BuildConfiguration();
var apiKey = configuration.GetOpenApiKey();
var engine = configuration.GetChatGptModel();

MessageWriter.PrintWelcomeMessage();

var chat = new Chat(engine, apiKey);

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