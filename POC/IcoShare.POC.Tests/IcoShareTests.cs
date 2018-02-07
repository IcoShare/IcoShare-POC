using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartContractEmulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace IcoShare.POC.Tests
{
    [TestClass]
    public class IcoShareTests
    {
        #region helper

        #endregion

        [TestInitialize]
        public void TestInit()
        {
            Runtime.Now = 1000;
        }

        private byte[] StartNewIcoShare()
        {
            byte[] tokenScriptHash = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".AsByteArray();

            byte[] icoShareId = "12345678912345".AsByteArray();

            IcoShareSmartContract.StartNewIcoShare(
                icoShareId: icoShareId,
                tokenScriptHash : tokenScriptHash,
                startTime: (BigInteger)1001,
                endTime: (BigInteger)5000,
                contributionBundle: (BigInteger)5000,
                minContribution: (BigInteger)100,
                maxContribution: (BigInteger)1000
                );

            return icoShareId;
        }

        [TestMethod]
        public void TestStartNewIcoShare()
        {
            byte[] icoShareId = StartNewIcoShare();

            Assert.IsNotNull(icoShareId);
            Assert.IsTrue(Storage.MemoryStorage.ContainsKey(icoShareId.AsString() + IcoShareSmartContract.POSTFIX_BUNDLE));
            Assert.IsTrue(Storage.MemoryStorage.ContainsKey(icoShareId.AsString() + IcoShareSmartContract.POSTFIX_CURRENTCONT));
            Assert.IsTrue(Storage.MemoryStorage.ContainsKey(icoShareId.AsString() + IcoShareSmartContract.POSTFIX_ENDDATE));
            Assert.IsTrue(Storage.MemoryStorage.ContainsKey(icoShareId.AsString() + IcoShareSmartContract.POSTFIX_MAXCONT));
            Assert.IsTrue(Storage.MemoryStorage.ContainsKey(icoShareId.AsString() + IcoShareSmartContract.POSTFIX_MINCONT));
            Assert.IsTrue(Storage.MemoryStorage.ContainsKey(icoShareId.AsString() + IcoShareSmartContract.POSTFIX_STARTDATE));
            Assert.IsTrue(Storage.MemoryStorage.ContainsKey(icoShareId.AsString() + IcoShareSmartContract.POSTFIX_STATUS));
            Assert.AreEqual(Storage.MemoryStorage[icoShareId.AsString() + IcoShareSmartContract.POSTFIX_STATUS], IcoShareSmartContract.ACTIVE.AsString());
        }

        [TestMethod]
        public void TestGetIcoShareStatus()
        {
            byte[] icoShareId = StartNewIcoShare();

            BigInteger value = IcoShareSmartContract.GetCurrentContribution(icoShareId);
            Assert.AreEqual(value, 0);
        }

        [TestMethod]
        public void GetCurrentContribution_ReturnCorrectAmount()
        {
            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributeValue = 50;
            byte[] icoShareId = StartNewIcoShare();

            var result = IcoShareSmartContract.SendContribution(icoShareId);
            BigInteger value = IcoShareSmartContract.GetCurrentContribution(icoShareId);
            Assert.AreEqual(value, 50);
        }

        [TestMethod]
        public void SendContribution_AddsCorrectAmount()
        {
            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributeValue = 50;

            byte[] icoShareId = StartNewIcoShare();

            var result = IcoShareSmartContract.SendContribution(icoShareId);
            Assert.IsTrue(result);
            Assert.IsTrue(Storage.MemoryStorage.ContainsKey(icoShareId.AsString() + "_" + ExecutionEngine.Sender.AsString()));
            Assert.AreEqual(Storage.MemoryStorage[icoShareId.AsString() + "_" + ExecutionEngine.Sender.AsString()], "50");
        }

        [TestMethod]
        public void SendContribution_AddsCorrectAmountDouble()
        {
            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributeValue = 50;

            byte[] icoShareId = StartNewIcoShare();

            IcoShareSmartContract.SendContribution(icoShareId);
            Assert.AreEqual(Storage.MemoryStorage[icoShareId.AsString() + "_" + ExecutionEngine.Sender.AsString()], "50");
            
            IcoShareSmartContract.SendContribution(icoShareId);
            Assert.AreEqual(Storage.MemoryStorage[icoShareId.AsString() + "_" + ExecutionEngine.Sender.AsString()], "100");
        }

        [TestMethod]
        public void SendContribution_AddsToContributors()
        {
            byte[] icoShareId = StartNewIcoShare();

            ExecutionEngine.Sender = "123123".AsByteArray();            
            IcoShareSmartContract.SendContribution(icoShareId);

            Assert.IsTrue(Storage.MemoryStorage[icoShareId.AsString() + IcoShareSmartContract.POSTFIX_CONTRIBUTORS].Contains("123123"));
            
            ExecutionEngine.Sender = "987654".AsByteArray();
            IcoShareSmartContract.SendContribution(icoShareId);
            Assert.IsTrue(Storage.MemoryStorage[icoShareId.AsString() + IcoShareSmartContract.POSTFIX_CONTRIBUTORS].Contains("987654"));
        }
        
        [TestMethod]
        public void SendContribution_ChecksIfFunded()
        {
            ExecutionEngine.Sender = "123123".AsByteArray();
            byte[] icoShareId = StartNewIcoShare();
            
            var key = icoShareId.AsString() + IcoShareSmartContract.POSTFIX_STATUS;
            var value = IcoShareSmartContract.FUNDED;
            Storage.Put(null, key.AsByteArray(), value);

            var result = IcoShareSmartContract.SendContribution(icoShareId);
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public void SendContribution_ChecksForMaximumAmount()
        {
            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributeValue = 1500;

            byte[] icoShareId = StartNewIcoShare();

            var result = IcoShareSmartContract.SendContribution(icoShareId);
            Assert.AreEqual(Storage.MemoryStorage[icoShareId.AsString() + "_" + ExecutionEngine.Sender.AsString()], "1000");
        }
        
        [TestMethod]
        public void SendContribution_RefundsTheAmountsThatMoreThanBundle()
        {
            byte[] icoShareId = StartNewIcoShare();

            ExecutionEngine.Sender = "1".AsByteArray();
            ExecutionEngine.ConributeValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);
            
            ExecutionEngine.Sender = "2".AsByteArray();
            ExecutionEngine.ConributeValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);
            
            ExecutionEngine.Sender = "3".AsByteArray();
            ExecutionEngine.ConributeValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);

            ExecutionEngine.Sender = "4".AsByteArray();
            ExecutionEngine.ConributeValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);

            ExecutionEngine.Sender = "5".AsByteArray();
            ExecutionEngine.ConributeValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);
            
            //TODO : Funded event called

            ExecutionEngine.Sender = "6".AsByteArray();
            ExecutionEngine.ConributeValue = 1000;
            var result = IcoShareSmartContract.SendContribution(icoShareId);
            Assert.IsFalse(result);

            //TODO : Refund event called 

            Assert.AreEqual(Storage.MemoryStorage[icoShareId.AsString() + IcoShareSmartContract.POSTFIX_BUNDLE], "5000");
        }

        [TestMethod]
        public void RefundUnsuccesfullIcoShare_ShouldRefund()
        {
            List<Tuple<string, BigInteger>> addresses = new List<Tuple<string, BigInteger>>();

            Runtime.Notified += (byte[][] messages) => {
                if (messages[0].AsString() == "REFUND")
                {
                    addresses.Add(new Tuple<string, BigInteger>(messages[1].AsString(), messages[2].AsBigInteger()));
                }
            }; 

            byte[] icoShareId = StartNewIcoShare();

            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributeValue = 500;
            IcoShareSmartContract.SendContribution(icoShareId);

            ExecutionEngine.Sender = "987654".AsByteArray();
            ExecutionEngine.ConributeValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);
            
            ExecutionEngine.Sender = "567891".AsByteArray();
            ExecutionEngine.ConributeValue = 750;
            IcoShareSmartContract.SendContribution(icoShareId);
            
            ExecutionEngine.Sender = "111".AsByteArray();
            IcoShareSmartContract.RefundUnsuccesfullIcoShare(icoShareId);

            var add1 = addresses.First(x => x.Item1 == "123123");
            var add2 = addresses.First(x => x.Item1 == "987654");
            var add3 = addresses.First(x => x.Item1 == "567891");

            Assert.IsTrue(add1 != null && add1.Item2 == 500);
            Assert.IsTrue(add2 != null && add2.Item2 == 1000);
            Assert.IsTrue(add3 != null && add3.Item2 == 750);
        }

    }
}
