using DrugiProjekat;
using SysProj1;

internal class Program
{
    public static async Task Main()
    {
        using CancellationTokenSource cts = new();
        Server server = new();

        
        Task serverTask = server.PokreniServerAsync(cts.Token);

        await Task.Delay(1000);

        Task stresor;
        for(int i=0; i<5; i++)
            stresor = StresSistem.stresor();


        Console.WriteLine("Pritisni ENTER za zaustavljanje");
        Console.ReadLine();

        cts.Cancel();
        server.ZaustaviServer();
        await serverTask;
    }
}