using System.Text.Json.Serialization;
using Marten.Schema;

namespace Ursa.Tokens;

public class AccessToken
{
    /// <summary>
    /// Identity separate to the (unique) hashed token value.
    /// Used for returning / querying via API etc.
    /// </summary>
    [Identity]
    public Guid TokenId { get; set; }

    [UniqueIndex]
    public string HashedToken { get; set; } = "";

    /// <summary>
    /// Full token as generated. Only available after initial creation.
    /// </summary>
    [JsonIgnore]
    public string RawToken { get; set; } = "";

    public string MaskedToken { get; set; } = "";

    public string UserId { get; set; } = "";

    public DateTimeOffset Expires { get; set; } = DateTimeOffset.MinValue;

    public Dictionary<string, object> Metadata { get; set; } = [];


    [JsonConstructor]
    public AccessToken()
    {
    }

    public static AccessToken Create(string userId, TimeSpan? duration = null, DateTimeOffset? expires = null, Dictionary<string, object>? metadata = null)
    {
        if (duration.HasValue && expires.HasValue)
        {
            throw new ArgumentException("Provide either duration or expires, but not both");
        }

        var token = Convert.ToBase64String(CryptoHelper.GenerateRandom(256));

        // no salt necessary as they're pseudo-random. We don't know the user id to retrieve a salt before hashing on retrieval anyway.
        var hashed = CryptoHelper.Hash(token, CryptoHelper.DefaultSalt); 

        return new AccessToken()
        {
            TokenId = Guid.NewGuid(),
            RawToken = token,
            HashedToken = hashed,
            MaskedToken = token[..8] + "*",
            UserId = userId,
            Expires = expires ?? (duration.HasValue 
                ? DateTimeOffset.UtcNow.Add(duration.Value) 
                : DateTimeOffset.UtcNow.AddDays(30)),
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}