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
        private string _icoShare1 = "bdf067b8-1908-45e7-84ea-4412a3603e79";
        private string _icoShare2 = "67b8bdf0-45e7-1908-4412-a3603e7984ea";

        [TestInitialize]
        public void TestInit()
        {
            Blockchain.Header = new Header(1000);
            Storage.MemoryStorage = new Dictionary<string, string>();
        }

        private byte[] StartNewIcoShare(
            string scriptHash = "5d7d82d4ebf8a24a3fcbc2f228c37687d474d0a6", 
            string icoShare = "bdf067b8-1908-45e7-84ea-4412a3603e79")
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
            Assert.AreEqual("50", Storage.MemoryStorage[icoShareId.AsString() + "_" + ExecutionEngine.Sender.AsString()]);
            
            IcoShareSmartContract.SendContribution(icoShareId);
            Assert.AreEqual("100", Storage.MemoryStorage[icoShareId.AsString() + "_" + ExecutionEngine.Sender.AsString()]);
        }

        [TestMethod]
        public void SendContribution_AddsToContributors()
        {
            byte[] icoShareId = StartNewIcoShare();

            ExecutionEngine.Sender = "123123".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 20;
            Assert.IsTrue(IcoShareSmartContract.SendContribution(icoShareId));

            Assert.AreEqual("123123", Storage.MemoryStorage[icoShareId.AsString() + IcoShareSmartContract.POSTFIX_CONTRIBUTORS]);
            
            ExecutionEngine.Sender = "987654".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 30;
            Assert.IsTrue(IcoShareSmartContract.SendContribution(icoShareId));

            Assert.AreEqual("123123_987654", Storage.MemoryStorage[icoShareId.AsString() + IcoShareSmartContract.POSTFIX_CONTRIBUTORS]);
        }


        [TestMethod]
        public void SendContribution_AddsToContributedSharesList()
        {
            byte[] icoShareId1 = StartNewIcoShare("AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y", _icoShare1);
            byte[] icoShareId2 = StartNewIcoShare("Jr6o664CWJKi1QRXjqeic2zRp8yAK2nJJp", _icoShare2);

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
            byte[] icoShareId1 = StartNewIcoShare("AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y", _icoShare1);

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
            byte[] icoShareId1 = StartNewIcoShare("AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y", _icoShare1);

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
            byte[] icoShareId1 = StartNewIcoShare("AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y", _icoShare1);

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

            ExecutionEngine.Sender = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 500;
            if (!IcoShareSmartContract.SendContribution(icoShareId)) Assert.Fail();

            ExecutionEngine.Sender = "Ki1QRXjqeic2zRp8yAK2nJJpJr6o664CWJ".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 1000;
            if (!IcoShareSmartContract.SendContribution(icoShareId)) Assert.Fail();

            ExecutionEngine.Sender = "JJpJr6o664CWJKi1QRXjqeic2zRp8yAK2n".AsByteArray();
            ExecutionEngine.ConributedNeoValue = 750;
            if (!IcoShareSmartContract.SendContribution(icoShareId)) Assert.Fail();

            ExecutionEngine.Sender = ExecutionEngine.ExecutingScriptHash;
            if (!IcoShareSmartContract.RefundUnsuccesfullIcoShare(icoShareId)) Assert.Fail();

            var add1 = addresses.First(x => x.Item1 == "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y");
            var add2 = addresses.First(x => x.Item1 == "Ki1QRXjqeic2zRp8yAK2nJJpJr6o664CWJ");
            var add3 = addresses.First(x => x.Item1 == "JJpJr6o664CWJKi1QRXjqeic2zRp8yAK2n");

            Assert.IsTrue(add1 != null && add1.Item2 == 500);
            Assert.IsTrue(add2 != null && add2.Item2 == 1000);
            Assert.IsTrue(add3 != null && add3.Item2 == 750);
        }

    }
}
