namespace SzkolenieAI.FunctionToolExamples;

internal static class WeatherApi
{
    public static string GetWeatherAsync(string city, string unit = "celsius")
    {
        Random random = new();
        var temperature = random.Next(-20, 40);
        return $"The current temperature in {city} is {temperature}°{unit}.";
    }
}