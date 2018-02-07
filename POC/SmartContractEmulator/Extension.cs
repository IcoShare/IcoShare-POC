using System.Numerics;

namespace SmartContractEmulator
{
    public static class Extension
    {
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

    }

}
