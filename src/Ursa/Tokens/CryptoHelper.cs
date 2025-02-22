using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Ursa.Tokens;

internal static class CryptoHelper
{
    public static byte[] DefaultSalt = [];

    public static byte[] GenerateRandom(int bits)
    {
        byte[] buffer = new byte[bits / 8];
        using (var rngCsp = RandomNumberGenerator.Create())
        {
            rngCsp.GetNonZeroBytes(buffer);
        }

        return buffer;
    }

    public static string Hash(string token, byte[] salt)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: token,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
    }
}