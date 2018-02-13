using SmartContractEmulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IcoShare.POC
{
    public class Contribution
    {
        public byte[] SenderAddress { get; set; }
        public byte[] IcoShareId { get; set; }
        public int Amount { get; set; }
    }

    public class IcoShare
    {
        public byte[] Status { get; set; }
        public byte[] IcoAddress { get; set; }
        public BigInteger StartDate { get; set; }
        public BigInteger EndData { get; set; }
        public BigInteger Bundle { get; set; }
        public BigInteger MinCount { get; set; }
        public BigInteger MaxCount { get; set; }
        public BigInteger CurrentContribution { get; set; }

        public static IcoShare NewIcoShare(byte[] status, byte[] icoAddress, byte[] startDate, byte[] endData, byte[] bundle, byte[] minCount, byte[] maxCount, byte[] CurrentContribution) {
            return new IcoShare {
                Bundle = bundle.AsBigInteger(),
                CurrentContribution = CurrentContribution.AsBigInteger(),
                EndData = endData.AsBigInteger(),
                IcoAddress = icoAddress,
                MaxCount = maxCount.AsBigInteger(),
                MinCount = minCount.AsBigInteger(),
                StartDate = startDate.AsBigInteger(),
                Status = status
            }; 
        }
    }
}
