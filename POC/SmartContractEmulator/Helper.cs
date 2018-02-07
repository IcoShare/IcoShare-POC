using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartContractEmulator
{
    public static class Helper
    {
        public static byte[] Concat(this byte[] first, byte[] second)
        {
            return string.Concat(first.AsString(), second.AsString()).AsByteArray();
        }
    }
}
