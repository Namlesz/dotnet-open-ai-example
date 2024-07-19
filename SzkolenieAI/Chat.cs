using System.ClientModel;
using System.Text;
using OpenAI.Chat;
using SzkolenieAI.Helpers;

namespace SzkolenieAI;

internal class Chat
{
    private const string SystemMessage =
        "Jesteś wielkim pisarzem, Adamem Mickiewiczem. Wszystkie twoje odpowiedzi mogą mieć maksymalnie 2 zdania.";

    private readonly ChatClient _client;
    private readonly List<ChatMessage> _chatMessages = [];
    private readonly StringBuilder _assistantResponse = new();
    private double _totalCost;

    private readonly ChatCompletionOptions _completionOptions = new()
    {
        MaxTokens = 150,
        Temperature = 0.5f
    };

    public Chat(string engine, string apiKey)
    {
        _client = new ChatClient(engine, apiKey);
        _chatMessages.Add(ChatMessage.CreateSystemMessage(SystemMessage));
    }

    public bool GetUserInput()
    {
        ColoredConsole.WriteUser("[YOU]: ");
        string? userInput;
        do
        {
            userInput = Console.ReadLine();
        } while (string.IsNullOrEmpty(userInput));

        if (userInput.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        _chatMessages.Add(ChatMessage.CreateUserMessage(userInput));
        return true;
    }

    public async Task ProcessChatSession()
    {
        ColoredConsole.WriteAssistant("[ASSISTANT]:");

        AsyncResultCollection<StreamingChatCompletionUpdate>
            updates = _client.CompleteChatStreamingAsync(_chatMessages, _completionOptions);

        _assistantResponse.Clear();
        await foreach (var update in updates)
        {
            foreach (var updatePart in update.ContentUpdate)
            {
                var msg = updatePart.Text;
                Console.Write(msg);
                _assistantResponse.Append(msg);
            }

            if (update is { Usage: not null })
            {
                var (inputTokens, outputTokens) = (update.Usage.InputTokens, update.Usage.OutputTokens);
                MessageWriter.WriteCosts(inputTokens, outputTokens);

                // TODO: REMOVE
                _totalCost += CostCalculator.CalculateTotalCost(inputTokens, outputTokens);
            }
        }

        _chatMessages.Add(ChatMessage.CreateAssistantMessage(_assistantResponse.ToString()));
    }

    public double GetTotalCost() => _totalCost;
}