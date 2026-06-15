using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugiProjekat
{
    public sealed class CacheEntry
    {
        public string Hash { get; }
        public DateTimeOffset ExpiresAt { get; }

        public CacheEntry(string hash, DateTimeOffset expiresAt)
        {
            Hash = hash;
            ExpiresAt = expiresAt;
        }

        public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;
    }
}
