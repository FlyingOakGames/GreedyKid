// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using System;
using System.Text;

namespace GreedyKid
{
    public static class HashHelper
    {
        public static string SHA1(string text)
        {
            byte[] buffer = Encoding.Default.GetBytes(text);
            string ret;
            using (System.Security.Cryptography.SHA1CryptoServiceProvider cryptoTransformSHA1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                ret = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            }
            return ret;
        }
    }
}
