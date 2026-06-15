using System.Net;
using System.Threading.Channels;

namespace DrugiProjekat
{
    internal class Server
    {

        private static string folderPath = "D:\\Ilija\\Faks\\uni\\III_GOD\\VI\\SysProg\\proj\\SysProj1\\fajlovi\\";
        private readonly HttpListener server;
        private readonly string url; // ovo mi je url servera 
        //private static ConcurrentQueue<ConcurentQueueElement> redRequesta = new ConcurrentQueue<ConcurentQueueElement>();
        private Channel<FileRequest> requestChannel;
        private readonly int brojRadnika=5;


        HashCache cache=new HashCache(TimeSpan.FromSeconds(30));

        // ogranicenje broja paralalnih obrada
        private static readonly SemaphoreSlim processingLimiter = new SemaphoreSlim(initialCount: 4, maxCount: 4);


        public Server(string url = "http://localhost:5050/")
        {
            this.url = url;

            server = new HttpListener();
            server.Prefixes.Add(url);

            requestChannel =
            Channel.CreateBounded<FileRequest>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleWriter = true,
                    SingleReader = false
                });

        }

        public async Task PokreniServerAsync(CancellationToken cancellationToken)
        {
            try
            {
                server.Start();

                Console.WriteLine("--- Server uspesno pokrenut ---");
                Console.WriteLine($"Slusam na: {url}");

                // spremam radnike
                Task[] radnici = Enumerable
                .Range(0, brojRadnika)
                .Select(radnikId =>
                    WorkerLoopAsync(
                        radnikId,
                        cancellationToken))
                .ToArray();


                while (!cancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext context;
                    Console.WriteLine("loop");
                    try
                    {
                        context = await server.GetContextAsync(); // nit se oslobadja dok ne primi context
                        await DodajRequestUKanalAsync(context);
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

        // workeri citaju iz kanala
        private async Task WorkerLoopAsync(int radnikId, CancellationToken cancellationToken)
        {
            await foreach (FileRequest request in 
                requestChannel.Reader.ReadAllAsync(cancellationToken))
            {
                await HashAndSend(radnikId,request,cancellationToken);
            }
        }


        // novo!!! 
        private async Task<FileRequest> HashAndSend(int radnikId,FileRequest request, CancellationToken cts)
        {
            Task<string> hashTask = cache.GetOrCreateAsync(request.Path,token => Hashing.HashFileAsync(request.Path, token),cts);

            await hashTask.ContinueWith(
                async completedTask =>
                {
                    HttpListenerResponse response = request.Context.Response;

                    try
                    {
                        if (completedTask.IsCanceled)
                        {
                            response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                            await SendToClientAsync(response, "Request cancelled");
                            return;
                        }

                        if (completedTask.IsFaulted)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            string hashVrednost = completedTask.Exception?.GetBaseException().Message ?? "Hashing failed";

                            await SendToClientAsync(response, hashVrednost);
                            return;
                        }

                        response.StatusCode = (int)HttpStatusCode.OK;
                        // upis
                        await SendToClientAsync(response, completedTask.Result);
                    }
                    finally
                    {
                        response.Close();
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default).Unwrap();

            return request;

        }

        private static async Task SendToClientAsync(HttpListenerResponse response, string text)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);

            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer);
        }


//private static async Task<string> ProcessRequestAsync(
//    int workerId,
//    string fileName,
//    CancellationToken cancellationToken)
//    {
//        // Sprecava izlazak iz root direktorijuma pomoću putanja kao ../../file.txt
//        fileName = Path.GetFileName(fileName);

//        if (string.IsNullOrWhiteSpace(fileName))
//        {
//            throw new ArgumentException("Naziv fajla nije prosledjen");
//        }

//        string filePath = Path.Combine(folderPath, fileName);

//        if (!File.Exists(filePath))
//        {
//            throw new FileNotFoundException(
//                $"Fajl '{fileName}' nije pronadjen",
//                filePath);
//        }

//        cancellationToken.ThrowIfCancellationRequested();

//        // Otvaranje fajla za asinhrono citanje
//        await using FileStream stream = new FileStream(
//            filePath,
//            FileMode.Open,
//            FileAccess.Read,
//            FileShare.Read,
//            bufferSize: 81920,
//            useAsync: true);

//        using SHA256 sha256 = SHA256.Create();

//        byte[] hashBytes = await sha256.ComputeHashAsync(
//            stream,
//            cancellationToken);

//        return Convert.ToHexString(hashBytes).ToLowerInvariant();
//    }

        

        private async Task DodajRequestUKanalAsync(HttpListenerContext context)
        {
            // priprema 
            string filename = context.Request.Url?.AbsolutePath.Trim('/');
            if (filename == null )
                filename = "";


            if (filename != "favicon.ico")
            {
                string targetPath = Path.Combine(folderPath, filename);

                FileRequest fileRequest = new FileRequest(targetPath, context);

                await requestChannel.Writer.WriteAsync(fileRequest);


            }
            else
            {
                //context.Response.StatusCode = (int)HttpStatusCode.NoContent; // 204
                context.Response.Close();
                return;

            }
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
