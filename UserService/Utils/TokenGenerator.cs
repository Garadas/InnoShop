using System;
using System.Security.Cryptography;

namespace UserService.Utils
{
    public static class TokenGenerator
    {
        public static string Create(int bytes = 32)
        {
            var data = RandomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
