using Microsoft.Extensions.Configuration;

namespace SzkolenieAI.Startup;

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
}