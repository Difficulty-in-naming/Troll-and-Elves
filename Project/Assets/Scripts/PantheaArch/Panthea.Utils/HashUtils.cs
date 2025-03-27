using System.Security.Cryptography;
using System.Text;

namespace Panthea.Utils
{
    public class HashUtils
    {
        public static string Generate32CharUUID(char[] chars)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                var data = new byte[32];
                rng.GetBytes(data);

                var result = new StringBuilder(32);
                foreach (var b in data)
                {
                    result.Append(chars[b % chars.Length]);
                }

                while (result[0] == '_')
                {
                    rng.GetBytes(data);
                    result.Clear();
                    foreach (var b in data)
                    {
                        result.Append(chars[b % chars.Length]);
                    }
                }

                return result.ToString();
            }
        }
    }
}