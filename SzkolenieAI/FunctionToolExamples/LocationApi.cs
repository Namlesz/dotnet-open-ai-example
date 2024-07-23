namespace SzkolenieAI.FunctionToolExamples;

internal static class LocationApi
{
    public static string GetLocationAsync()
    {
        string[] cities = ["Warsaw", "Krakow", "Lodz", "Wroclaw", "Poznan", "Gdansk", "Szczecin", "Bydgoszcz", "Lublin", "Katowice"];
        var random = new Random();
        var index = random.Next(cities.Length);

        return cities[index];
    }
}