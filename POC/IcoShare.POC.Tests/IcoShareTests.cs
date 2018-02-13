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
        [TestInitialize]
        public void TestInit()
        {
            Blockchain.Header = new Header(1000);
            Storage.MemoryStorage = new Dictionary<string, string>();
        }

        private byte[] StartNewIcoShare(
            string scriptHash = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y", 
            string icoShare = "12345678912345")
        {
            byte[] tokenScriptHash = scriptHash.AsByteArray();

            byte[] icoShareId = icoShare.AsByteArray();
            
            bool isStarted = IcoShareSmartContract.StartNewIcoShare(
                icoShareId: icoShareId,
                tokenScriptHash: tokenScriptHash,
                startTime: (BigInteger)1100,
                endTime: (BigInteger)5000,
                contributionBundle: (BigInteger)5000,
                minContribution: (BigInteger)100,
                maxContribution: (BigInteger)1000
                );

            if (!isStarted) Assert.Fail();

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
        public void GetCurrentContribution_ReturnsCorrectAmount()
        {
            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 50;
            byte[] icoShareId = StartNewIcoShare();

            var result = IcoShareSmartContract.SendContribution(icoShareId);
            BigInteger value = IcoShareSmartContract.GetCurrentContribution(icoShareId);
            Assert.AreEqual(value, 50);
        }

        [TestMethod]
        public void SendContribution_AddsCorrectAmount()
        {
            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 50;

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
            ExecutionEngine.ConributedNeoValue = 50;

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

            Assert.AreEqual(Storage.MemoryStorage[icoShareId.AsString() + IcoShareSmartContract.POSTFIX_CONTRIBUTORS], "123123");
            
            ExecutionEngine.Sender = "987654".AsByteArray();
            IcoShareSmartContract.SendContribution(icoShareId);

            Assert.AreEqual(Storage.MemoryStorage[icoShareId.AsString() + IcoShareSmartContract.POSTFIX_CONTRIBUTORS], "123123_987654");
        }


        [TestMethod]
        public void SendContribution_AddsToContributedSharesList()
        {
            byte[] icoShareId1 = StartNewIcoShare("AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y", "12345678912345");
            byte[] icoShareId2 = StartNewIcoShare("Jr6o664CWJKi1QRXjqeic2zRp8yAK2nJJp", "98765123456789");

            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 10;
            IcoShareSmartContract.SendContribution(icoShareId1);
            ExecutionEngine.ConributedNeoValue = 15;
            IcoShareSmartContract.SendContribution(icoShareId2);

            ExecutionEngine.Sender = "567567".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 20;
            IcoShareSmartContract.SendContribution(icoShareId1);

            Assert.AreEqual(
                icoShareId1.AsString() + "_" + icoShareId2.AsString(),
                Storage.MemoryStorage["123123" + IcoShareSmartContract.POSTFIX_CONTRIBUTEDSHARES]
            );

            Assert.AreEqual(
                icoShareId1.AsString(),
                Storage.MemoryStorage["567567" + IcoShareSmartContract.POSTFIX_CONTRIBUTEDSHARES]
            );
        }

        [TestMethod]
        public void SendContribution_AddsToContributedSharesListOnlyOnce()
        {
            byte[] icoShareId1 = StartNewIcoShare("AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y", "12345678912345");

            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 10;
            IcoShareSmartContract.SendContribution(icoShareId1);
            IcoShareSmartContract.SendContribution(icoShareId1);
            
            Assert.AreEqual(
                icoShareId1.AsString(),
                Storage.MemoryStorage["123123" + IcoShareSmartContract.POSTFIX_CONTRIBUTEDSHARES]);
        }

        [TestMethod]
        public void SendContribution_AddsToContributorsListOnlyOnce()
        {
            byte[] icoShareId1 = StartNewIcoShare("AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y", "12345678912345");

            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 10;
            if (!IcoShareSmartContract.SendContribution(icoShareId1)) Assert.Fail();
            if (!IcoShareSmartContract.SendContribution(icoShareId1)) Assert.Fail();

            Assert.AreEqual(
                ExecutionEngine.Sender.AsString(), 
                Storage.MemoryStorage[icoShareId1.AsString() + IcoShareSmartContract.POSTFIX_CONTRIBUTORS]);
        }

        [TestMethod]
        public void SendContribution_0ContributionReturnsFalse()
        {
            byte[] icoShareId1 = StartNewIcoShare("AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y", "12345678912345");

            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 0;

            var result = IcoShareSmartContract.SendContribution(icoShareId1);
            Assert.IsFalse(result);
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
            ExecutionEngine.ConributedNeoValue = 1500;

            byte[] icoShareId = StartNewIcoShare();

            var result = IcoShareSmartContract.SendContribution(icoShareId);
            Assert.AreEqual(Storage.MemoryStorage[icoShareId.AsString() + "_" + ExecutionEngine.Sender.AsString()], "1000");
        }
        
        [TestMethod]
        public void SendContribution_RefundsTheAmountsThatMoreThanBundle()
        {
            List<Tuple<string, BigInteger>> addresses = new List<Tuple<string, BigInteger>>();

            Runtime.Notified += (byte[][] messages) => {
                if (messages[0].AsString() == "REFUND")
                {
                    addresses.Add(new Tuple<string, BigInteger>(messages[1].AsString(), messages[2].AsBigInteger()));
                }
            };

            byte[] icoShareId = StartNewIcoShare();

            ExecutionEngine.Sender = "1".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);
            
            ExecutionEngine.Sender = "2".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);
            
            ExecutionEngine.Sender = "3".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);

            ExecutionEngine.Sender = "4".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);

            ExecutionEngine.Sender = "5".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 1000;
            IcoShareSmartContract.SendContribution(icoShareId);
            
            //TODO : Funded event called

            ExecutionEngine.Sender = "6".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 1000;
            var result = IcoShareSmartContract.SendContribution(icoShareId);
            Assert.IsFalse(result);
            
            var add1 = addresses.First(x => x.Item1 == "6");
            Assert.IsTrue(add1 != null && add1.Item2 == 1000);

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
            ExecutionEngine.ConributedNeoValue = 500;
            if (!IcoShareSmartContract.SendContribution(icoShareId)) Assert.Fail();

            ExecutionEngine.Sender = "987654".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 1000;
            if (!IcoShareSmartContract.SendContribution(icoShareId)) Assert.Fail();

            ExecutionEngine.Sender = "567891".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 750;
            if (!IcoShareSmartContract.SendContribution(icoShareId)) Assert.Fail();

            ExecutionEngine.Sender = ExecutionEngine.ExecutingScriptHash;
            if (!IcoShareSmartContract.RefundUnsuccesfullIcoShare(icoShareId)) Assert.Fail();

            var add1 = addresses.First(x => x.Item1 == "123123");
            var add2 = addresses.First(x => x.Item1 == "987654");
            var add3 = addresses.First(x => x.Item1 == "567891");

            Assert.IsTrue(add1 != null && add1.Item2 == 500);
            Assert.IsTrue(add2 != null && add2.Item2 == 1000);
            Assert.IsTrue(add3 != null && add3.Item2 == 750);
        }

    }
}
