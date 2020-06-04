using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Business.Infastructure
{
    public static class EncryptionHelper
    {
        public static string EncryptText(string text)
        {
            // Hashes input string using SHA512
            SHA512 sha = new SHA512Managed();
            var data = Encoding.UTF8.GetBytes(text);
            var hash = sha.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }
    }
}