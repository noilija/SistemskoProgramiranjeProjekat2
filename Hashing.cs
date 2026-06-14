using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DrugiProjekat
{
    internal class Hashing
    {
        public static string HashFileAndReturn(string path)
        {
            try
            {

                using (SHA256 sha256 = SHA256.Create())
                {
                    using (FileStream stream = File.OpenRead(path))
                    {
                        // hesiranje 
                        byte[] hashBytes = sha256.ComputeHash(stream);
                        string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                        // prikaz u konzoli
                        Console.WriteLine("\n--------HASHOVANJE");
                        string imeFajla = Path.GetFileName(path);
                        Console.WriteLine($"Fajl: {imeFajla}");
                        Console.WriteLine($"SHA256 Heš: {hashString}");
                        Console.WriteLine("----------------\n");

                        return hashString;


                    }
                }
            }
            catch (FileNotFoundException)
            {
                // ako nema fajla
                Console.WriteLine($"HESOVANJE\n Greška: Fajl na putanji '{path}' nije pronadjen.");
                return null;

            }
        }

    }
}
