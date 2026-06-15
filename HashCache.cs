using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DrugiProjekat
{
    internal sealed class HashCache
    {

        private readonly ConcurrentDictionary<string, Lazy<Task<CacheEntry>>> cacheElements = new();

        private readonly TimeSpan zivotniVek;

        public HashCache(TimeSpan zivotniVek)
        {
            this.zivotniVek = zivotniVek;
        }


        public async Task<string> GetOrCreateAsync(
         string filePath,
         Func<CancellationToken, Task<string>> hashFactory,
         CancellationToken cancellationToken)
        {
            string cacheKey = Path.GetFullPath(filePath);

            while (true)
            {
                // spremamo lazy obradu ali ne pokrecemo
                Lazy<Task<CacheEntry>> newEntry = new(
                    async () =>
                    {
                        Console.WriteLine($"CACHE MISS: {cacheKey}");

                        // racunanje hesa
                        string hash = await hashFactory(cancellationToken);

                        // vrati hes
                        return new CacheEntry(
                            hash,
                            DateTimeOffset.UtcNow.Add(zivotniVek));
                    });

                // ako postoji uzmi je 
                // ako ne , newEntry
                Lazy<Task<CacheEntry>> actualEntry =
                    cacheElements.GetOrAdd(cacheKey, newEntry);

                try
                {
                    // pokreni oobradu
                    CacheEntry entry = await actualEntry.Value;

                    if (!entry.IsExpired)
                    {
                        Console.WriteLine($"CACHE HIT: {cacheKey}");
                        Console.WriteLine($"Hash vrednost: {entry.Hash}");
                        return entry.Hash;
                    }

                    // isteklo
                    cacheElements.TryRemove(cacheKey, out _);

                    // while ponovo pokusava da napravi novu stavku
                }
                catch
                {
                    // ne cuvamo task koji se zavrsio greskom
                    cacheElements.TryRemove(cacheKey, out _);
                    throw;
                }
            }
        }

        public void RemoveExpiredEntries()
        {
            foreach (var cacheElement in cacheElements)
            {
                Lazy<Task<CacheEntry>> tmp = cacheElement.Value;

                // obrada nije pokrenuta znaci skip
                if (!tmp.IsValueCreated)
                    continue;

                Task<CacheEntry> task = tmp.Value;

                // obrada nije zavrsena znaci skip
                if (!task.IsCompletedSuccessfully)
                    continue;

                // stavka istekla znaci brisi
                if (task.Result.IsExpired)
                    cacheElements.TryRemove(cacheElement.Key, out _);
            }
        }

        public void Clear()
        {
            cacheElements.Clear();
        }
    }

   
}
