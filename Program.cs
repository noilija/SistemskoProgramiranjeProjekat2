using DrugiProjekat;

internal class Program
{
    public static async Task Main()
    {
        using CancellationTokenSource cts = new();

        Server server = new();

        Task serverTask = server.PokreniServerAsync(cts.Token);

        Console.WriteLine("Pritisni ENTER za zaustavljanje.");
        Console.ReadLine();

        cts.Cancel();
        server.ZaustaviServer();

        await serverTask;
    }
}