using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace SzkolenieAI.Startup;

// TODO: Get all config at once
internal static class SettingsConfiguration
{
    public static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();
    }

    public static string GetOpenApiKey(this IConfiguration config)
    {
        var apiKey = config["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentNullException(apiKey, "API key cannot be null or empty.");
        }

        return apiKey;
    }

    public static string GetChatGptModel(this IConfiguration config)
    {
        var model = config["OpenAI:Model"];
        if (string.IsNullOrEmpty(model))
        {
            throw new ArgumentNullException(model, "Model cannot be null or empty.");
        }

        return model;
    }

    public static double GetInputTokenCosts(this IConfiguration config)
    {
        var inputTokenPrice = config["OpenAI:InputTokenPrice"];
        if (string.IsNullOrEmpty(inputTokenPrice))
        {
            throw new ArgumentNullException(inputTokenPrice, "Input token price cannot be null or empty.");
        }

        return double.Parse(inputTokenPrice, CultureInfo.InvariantCulture);
    }

    public static double GetOutputTokenCosts(this IConfiguration config)
    {
        var outputTokenPrice = config["OpenAI:OutputTokenPrice"];
        if (string.IsNullOrEmpty(outputTokenPrice))
        {
            throw new ArgumentNullException(outputTokenPrice, "Output token price cannot be null or empty.");
        }

        return double.Parse(outputTokenPrice, CultureInfo.InvariantCulture);
    }
}