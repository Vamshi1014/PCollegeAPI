using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;

namespace Flyurdreamcommands.Helpers
{
    public static class CompressData
    {
        public static byte[] CompressString(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("The string cannot be null or empty.", nameof(text));

            byte[] bytes = Encoding.UTF8.GetBytes(text);

            using (var outputStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    zipStream.Write(bytes, 0, bytes.Length);
                }

                return outputStream.ToArray();
            }
        }
              
    }
    public static class CurrencyHelper
    {
        public static (string currency, int amount) ProcessCurrencyValue(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Equals("Not Available", StringComparison.OrdinalIgnoreCase))
            {
                return (null, 0);
            }

            string[] splitValue = input.Split(' ');
            if (splitValue.Length == 2 && int.TryParse(splitValue[1], out int amount))
            {
                return (splitValue[0], amount);
            }

            return (null, 0);
        }
    }

  
}
