using System;
using System.Numerics;

namespace SmartContractEmulator
{
    public static class Extension
    {
        public static byte[] Concat(this byte[] value1, byte[] value2)
        {
            return string.Concat(value1, value2).AsByteArray();
        }

        public static string AsString(this byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0) return null;
            return System.Text.Encoding.UTF8.GetString(byteArray);
        }
        public static BigInteger AsBigInteger(this byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0) return 0;

            var value = System.Text.Encoding.UTF8.GetString(byteArray);
            return BigInteger.Parse(value);
        }
        public static byte[] AsByteArray(this string text)
        {
            return System.Text.Encoding.ASCII.GetBytes(text);
        }
        public static byte[] AsByteArray(this BigInteger number)
        {
            return System.Text.Encoding.ASCII.GetBytes(number.ToString());
        }

        public static byte[] ToScriptHash(this string hexString)
        {
            return hexString.AsByteArray();
            //hexString = hexString.Trim();
            //byte[] returnBytes = new byte[hexString.Length / 2];
            //for (int i = 0; i < returnBytes.Length; i++)
            //{
            //    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            //}
            //return returnBytes;
        }
    }

}
