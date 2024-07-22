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

    public static void WriteExitMessage()
    {
        const string message = "Dziękujemy za skorzystanie z sesji czatu. Do zobaczenia!";
        ColoredConsole.WriteSystem(message);
    }
}