using System.ClientModel;
using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using SzkolenieAI.FunctionToolExamples;
using SzkolenieAI.Helpers;

namespace SzkolenieAI;

internal class Chat
{
    private const string SystemMessage =
        "Jesteś wielkim pisarzem, Adamem Mickiewiczem.";

    private readonly ChatClient _client;
    private readonly List<ChatMessage> _chatMessages = [];
    private readonly CostCalculator _costCalculator;
    private readonly StringBuilder _assistantResponse = new();
    private double _totalCost;
    private int _inputTokens;
    private int _outputTokens;

    private static readonly ChatTool getCurrenLocationTool = ChatTool.CreateFunctionTool(
        functionName: nameof(LocationApi.GetLocationAsync),
        functionDescription: "Get user current location"
    );

    private static readonly ChatTool getWeatherTool = ChatTool.CreateFunctionTool(
        functionName: nameof(WeatherApi.GetWeatherAsync),
        functionDescription: "Get the current weather in a given city",
        functionParameters: BinaryData.FromString
        ("""
          {
                    "type": "object",
                    "properties": {
                        "location": {
                            "type": "string",
                            "description": "The city"
                        },
                        "unit": {
                            "type": "string",
                            "enum": [ "celsius", "fahrenheit" ],
                            "description": "The temperature unit to use. Infer this from the specified location."
                        }
                    },
                    "required": [ "location" ]                                         
          }
         """)
    );

    private readonly ChatCompletionOptions _completionOptions = new()
    {
        MaxTokens = 250,
        Temperature = 1.0f,
        Tools = { getWeatherTool, getCurrenLocationTool }
    };

    public Chat(string engine, string apiKey, CostCalculator costCalculator)
    {
        _client = new ChatClient(engine, apiKey);
        _chatMessages.Add(ChatMessage.CreateSystemMessage(SystemMessage));
        _costCalculator = costCalculator;
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
        bool requiresAction;
        do
        {
            requiresAction = false;
            Dictionary<int, string> indexToToolCallId = [];
            Dictionary<int, string> indexToFunctionName = [];
            Dictionary<int, StringBuilder> indexToFunctionArguments = [];

            AsyncResultCollection<StreamingChatCompletionUpdate>
                chatUpdates = _client.CompleteChatStreamingAsync(_chatMessages, _completionOptions);

            _assistantResponse.Clear();
            await foreach (var chatUpdate in chatUpdates)
            {
                if (chatUpdate is { Usage: not null })
                {
                    (_inputTokens, _outputTokens) = (chatUpdate.Usage.InputTokens, chatUpdate.Usage.OutputTokens);
                }

                // Accumulate the text content as new updates arrive.
                foreach (var contentPart in chatUpdate.ContentUpdate)
                {
                    var msg = contentPart.Text;
                    Console.Write(msg);
                    _assistantResponse.Append(msg);
                }

                // Build the tool calls as new updates arrive.
                foreach (var toolCallUpdate in chatUpdate.ToolCallUpdates)
                {
                    // Keep track of which tool call ID belongs to this update index.
                    if (toolCallUpdate.Id is not null)
                    {
                        indexToToolCallId[toolCallUpdate.Index] = toolCallUpdate.Id;
                    }

                    // Keep track of which function name belongs to this update index.
                    if (toolCallUpdate.FunctionName is not null)
                    {
                        indexToFunctionName[toolCallUpdate.Index] = toolCallUpdate.FunctionName;
                    }

                    // Keep track of which function arguments belong to this update index,
                    // and accumulate the arguments string as new updates arrive.
                    if (toolCallUpdate.FunctionArgumentsUpdate is not null)
                    {
                        var argumentsBuilder = indexToFunctionArguments.TryGetValue(toolCallUpdate.Index, out var existingBuilder)
                            ? existingBuilder
                            : new StringBuilder();
                        argumentsBuilder.Append(toolCallUpdate.FunctionArgumentsUpdate);
                        indexToFunctionArguments[toolCallUpdate.Index] = argumentsBuilder;
                    }
                }

                switch (chatUpdate.FinishReason)
                {
                    case ChatFinishReason.Stop:
                    {
                        _chatMessages.Add(new AssistantChatMessage(_assistantResponse.ToString()));
                        Console.WriteLine();
                        break;
                    }

                    case ChatFinishReason.ToolCalls:
                    {
                        // First, collect the accumulated function arguments into complete tool calls to be processed
                        List<ChatToolCall> toolCalls = [];
                        foreach (var (index, toolCallId) in indexToToolCallId)
                        {
                            var toolCall = ChatToolCall.CreateFunctionToolCall(
                                toolCallId,
                                indexToFunctionName[index],
                                indexToFunctionArguments[index].ToString());

                            toolCalls.Add(toolCall);
                        }

                        // Next, add the assistant message with tool calls to the conversation history.
                        var content = _assistantResponse.Length > 0 ? _assistantResponse.ToString() : null;
                        _chatMessages.Add(new AssistantChatMessage(toolCalls, content));

                        // Then, add a new tool message for each tool call to be resolved.
                        foreach (var toolCall in toolCalls)
                        {
                            switch (toolCall.FunctionName)
                            {
                                case nameof(LocationApi.GetLocationAsync):
                                {
                                    var toolResult = LocationApi.GetLocationAsync();
                                    _chatMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                    break;
                                }

                                case nameof(WeatherApi.GetWeatherAsync):
                                {
                                    // The arguments that the model wants to use to call the function are specified as a
                                    // stringified JSON object based on the schema defined in the tool definition. Note that
                                    // the model may hallucinate arguments too. Consequently, it is important to do the
                                    // appropriate parsing and validation before calling the function.
                                    using var argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                    var hasLocation = argumentsJson.RootElement.TryGetProperty("location", out var location);
                                    var hasUnit = argumentsJson.RootElement.TryGetProperty("unit", out var unit);

                                    if (!hasLocation)
                                    {
                                        throw new ArgumentNullException(nameof(location), "The location argument is required.");
                                    }

                                    var toolResult = hasUnit
                                        ? WeatherApi.GetWeatherAsync(location.GetString(), unit.GetString())
                                        : WeatherApi.GetWeatherAsync(location.GetString());
                                    _chatMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                    break;
                                }
                            }
                        }

                        requiresAction = true;
                        break;
                    }

                    case ChatFinishReason.Length:
                        Console.WriteLine();
                        ColoredConsole.WriteInfo("Limit słów osiągnięty. Abym mógł kontynuować, wpisz 'kontynuuj'.");
                        _chatMessages.Add(new AssistantChatMessage(_assistantResponse.ToString()));

                        break;

                    case ChatFinishReason.ContentFilter:
                        throw new NotImplementedException("Omitted content due to a content filter flag.");

                    case ChatFinishReason.FunctionCall:
                        throw new NotImplementedException("Deprecated in favor of tool calls.");

                    case null:
                        break;
                }
            }
        } while (requiresAction);

        _costCalculator.WriteCosts(_inputTokens, _outputTokens);
        _totalCost += _costCalculator.CalculateTotalCost(_inputTokens, _outputTokens);
        _inputTokens = 0;
        _outputTokens = 0;
    }

    public double GetTotalCost() => _totalCost;
}