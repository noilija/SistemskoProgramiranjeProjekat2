using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrugiProjekat
{
    internal class Server
    {

        private static string folderPath = "D:\\Ilija\\Faks\\uni\\III_GOD\\VI\\SysProg\\proj\\SysProj1\\fajlovi\\";
        private readonly HttpListener server;
        private readonly string url; // ovo mi je url servera 
        private static ConcurrentQueue<ConcurentQueueElement> redRequesta = new ConcurrentQueue<ConcurentQueueElement>();




        public Server(string url = "http://localhost:5050/")
        {
            this.url = url;

            server = new HttpListener();
            server.Prefixes.Add(url);
        }

        public async Task PokreniServerAsync(CancellationToken cancellationToken)
        {
            try
            {
                server.Start();

                Console.WriteLine("--- Server uspesno pokrenut ---");
                Console.WriteLine($"Slusam na: {url}");

                while (!cancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext context;

                    try
                    {
                        context = await server.GetContextAsync();
                    }
                    catch (HttpListenerException)
                        when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                        when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await DodajRequestURed(context);
                }
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine(
                    $"Greska prilikom rada servera: {ex.Message}");
            }
            finally
            {
                if (server.IsListening)
                {
                    server.Stop();
                }

                server.Close();

                Console.WriteLine("Server je zaustavljen.");
            }
        }
        private static void DodajRequestURed(HttpListenerContext context)
        {
            string filename = context.Request.Url?.AbsolutePath.Trim('/') ?? "";

            string targetPath = Path.Combine(folderPath, filename);

            ConcurentQueueElement queueElement =
                new ConcurentQueueElement(targetPath, context);

            redRequesta.Enqueue(queueElement);
        }


        public void ZaustaviServer()
        {
            if (server.IsListening)
            {
                server.Stop();
            }
        }
    }
}
