using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace SysProj1
{
    internal class StresSistem
    {
        private static readonly HttpClient klijent = new HttpClient();



        public static async Task stresor()
        {

            Console.WriteLine("Stresor pokrenut:");
            try
            {
                var odgovor = "";
                Queue<string> listaZahteva = new Queue<string>();

                // stres zahtevi
                listaZahteva.Enqueue("http://localhost:5050/a1.txt");
                listaZahteva.Enqueue("http://localhost:5050/a2.txt");
                listaZahteva.Enqueue("http://localhost:5050/a3.txt");
                listaZahteva.Enqueue("http://localhost:5050/a4.txt");
                listaZahteva.Enqueue("http://localhost:5050/a5.txt");
                listaZahteva.Enqueue("http://localhost:5050/a1.txt");
                listaZahteva.Enqueue("http://localhost:5050/a1.txt");
                listaZahteva.Enqueue("http://localhost:5050/a2.txt");
                listaZahteva.Enqueue("http://localhost:5050/a1.txt");

                //listaZahteva.Enqueue("http://localhost:5050/a1.txt");
                //listaZahteva.Enqueue("http://localhost:5050/cisco.exe");
                //listaZahteva.Enqueue("http://localhost:5050/audacity.exe");
                //listaZahteva.Enqueue("http://localhost:5050/audacity.exe");
                //listaZahteva.Enqueue("http://localhost:5050/a1.txt");
                //listaZahteva.Enqueue("http://localhost:5050/cisco.exe");
                //listaZahteva.Enqueue("http://localhost:5050/audacity.exe");
                //listaZahteva.Enqueue("http://localhost:5050/audacity.exe");
                //listaZahteva.Enqueue("http://localhost:5050/dotnet.exe");


                // slanje GET zahteva serveru
                string tmp;
                while (listaZahteva.Count > 0)
                {
                    tmp = listaZahteva.Dequeue();
                    string imeFajla = Path.GetFileName(tmp);
                    Console.WriteLine($"\n********** STRESOR {imeFajla} *********");
                    odgovor = await klijent.GetStringAsync(tmp);
                    Console.WriteLine("Odgovor servera:");
                    Console.WriteLine(odgovor);

                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nGreska prilikom slanja!");
                Console.WriteLine("Poruka: {0} ", e.Message);
            }
        }
    }
}
