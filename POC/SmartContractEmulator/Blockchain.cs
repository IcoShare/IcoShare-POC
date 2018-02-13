using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartContractEmulator
{
    public class Header
    {
        uint _timestamp;
        public Header(uint timestamp)
        {
            _timestamp = timestamp;
        }

        public byte[] Hash { get; }
        public uint Version { get; }
        public byte[] PrevHash { get; }
        public byte[] MerkleRoot { get; }
        public uint Timestamp => _timestamp;
        public uint Index { get; }
        public ulong ConsensusData { get; }
        public byte[] NextConsensus { get; }
    }

    public class Blockchain
    {
        private static Header _header;

        //For unit test
        public static Header Header { get { return _header; } set { _header = value; } }

        public Blockchain(Header header)
        {
            _header = header;
        }

        public static Header GetHeader(uint height)
        {
            return _header;
        }

        public static uint GetHeight()
        {
            return 0;
        }
    }
}
