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
        public static async Task<string> HashFileAsync(string path,  CancellationToken cancellationToken)
        {
            await using FileStream stream = new(path, FileMode.Open, FileAccess.Read,FileShare.Read, bufferSize: 81920, useAsync: true);

            using SHA256 sha256 = SHA256.Create();

            byte[] hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
