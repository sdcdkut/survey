using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Surveyapp.Services;

public class InMemoryTicketStore : ITicketStore
{
    private const string KeyPrefix = "AuthSessionStore-";
    //private IDistributedCache _cache;
    private readonly IMemoryCache _cache;
    
    public InMemoryTicketStore(/*RedisCacheOptions options*/ IMemoryCache cache)
    {
        //_cache = new RedisCache(options);
        _cache = cache;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var guid = Guid.NewGuid();
        var key = KeyPrefix + guid;
        await RenewAsync(key, ticket);
        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var options = new DistributedCacheEntryOptions();
        var expiresUtc = ticket.Properties.ExpiresUtc;
        if (expiresUtc.HasValue)
        {
            options.SetAbsoluteExpiration(expiresUtc.Value);
        }

        //byte[] val = SerializeToBytes(ticket);
        _cache.Set(key, ticket, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow =
                TimeSpan.FromHours(12),
            Size = 322302030000000000
        });
        return Task.FromResult(0);
    }

    public Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        //byte[] bytes = null;
        //bytes = _cache.Get(key) as byte[];
        var ticket = _cache.Get(key) as AuthenticationTicket;
        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.FromResult(0);
    }

    private static byte[] SerializeToBytes(AuthenticationTicket source)
    {
        return TicketSerializer.Default.Serialize(source);
    }

    private static AuthenticationTicket DeserializeFromBytes(byte[] source)
    {
        return source == null ? null : TicketSerializer.Default.Deserialize(source);
    }
}

