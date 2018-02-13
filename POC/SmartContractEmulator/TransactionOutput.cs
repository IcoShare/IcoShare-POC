using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartContractEmulator
{
    public class TransactionOutput
    {
        public byte[] AssetId { get; set; }
        public byte[] ScriptHash { get; set; }
        public ulong Value { get; set; }
    }
}
