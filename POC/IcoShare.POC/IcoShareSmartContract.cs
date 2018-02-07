using SmartContractEmulator;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace IcoShare.POC
{
    public class IcoShareSmartContract : SmartContract
    {
        public static readonly byte[] Owner = "111".AsByteArray();

        public static readonly char POSTFIX_A = 'A';
        public static readonly char POSTFIX_STATUS = 'B';
        public static readonly char POSTFIX_STARTDATE = 'C';
        public static readonly char POSTFIX_ENDDATE = 'D';
        public static readonly char POSTFIX_BUNDLE = 'E';
        public static readonly char POSTFIX_MINCONT = 'F';
        public static readonly char POSTFIX_MAXCONT = 'G';
        public static readonly char POSTFIX_CURRENTCONT = 'H'; 
        public static readonly char POSTFIX_CONTRIBUTORS = 'J';
        public static readonly char POSTFIX_TOKENHASH = 'K';
        
        public static readonly byte[] ACTIVE = { 31, 32 };
        public static readonly byte[] FUNDED = { 32, 33 };
        public static readonly byte[] NOTFUNDED = { 33, 34 };

        public const int IdLenght = 14;
        public const int SenderAddresLenght = 6; //TODO : Set this to corrent length
        
        //[DisplayName("transfer")]
        public static event Action<byte[]> Funded;
        public static event Action<byte[], BigInteger> Refund;

        #region Helper 
        private static BigInteger Now()
        {
            //TODO : 
            return Runtime.Now;
        }
        private static byte[] GetSender()
        {
            return ExecutionEngine.GetSender();
        }
        private static bool IsOwner()
        {
            return Runtime.CheckWitness(Owner);
        }
        private static ulong GetContributeValue()
        {
            return ExecutionEngine.GetContibuteValue();
        }

        private static byte[] GetFromStorage(byte[] storageKey, char postfix)
        {
            string k =  storageKey.AsString() + postfix;
            return Storage.Get(Storage.CurrentContext, k.AsByteArray());
        }
        private static byte[] GetFromStorage(byte[] storageKey)
        {
            return Storage.Get(Storage.CurrentContext, storageKey);
        }
        private static byte[][] GetListFromStorage(byte[] storageKey, int listItemSize)
        {
            string list = GetFromStorage(storageKey).AsString();

            listItemSize = listItemSize + 1; //for seperator
            var len = (list.Length + 1 ) / listItemSize;

            byte[][] liste = new byte[len][];

            for (int i = 0; i < len;i++)
            {
                liste[i] = list.Substring(i * listItemSize, listItemSize - 1 ).AsByteArray();
            }

            return liste;
        }

        private static void PutOnStorage(byte[] storageKey, byte[] value)
        {
            Storage.Put(Storage.CurrentContext, storageKey, value);
        }
        private static void PutOnStorage(byte[] storageKey, string value)
        {
            PutOnStorage(storageKey, value.AsByteArray());
        }
        private static void PutOnStorage(byte[] storageKey, char postfix, byte[] value)
        {
            string k = string.Concat(storageKey.AsString(),postfix);
            Storage.Put(Storage.CurrentContext, k.AsByteArray(), value);
        }
        private static void PutOnStorage(byte[] storageKey, char postfix, string value)
        {
            PutOnStorage(storageKey, postfix, value.AsByteArray());
        }
        private static void PutItemOnStorageList(byte[] storageKey, char postfix, byte[] value)
        {
            var item = GetFromStorage(storageKey, postfix).AsString() ?? "";

            if (!string.IsNullOrEmpty(item))
                item = string.Concat(item, "_");

            item = string.Concat(item, value.AsString());

            PutOnStorage(storageKey, postfix, item.AsByteArray());
        }

        private static byte[] IntToBytes(BigInteger value)
        {
            byte[] buffer = value.ToByteArray();
            return buffer;
        }
        private static BigInteger BytesToInt(byte[] array)
        {
            return array.AsBigInteger() + 0;
        }

        private static byte[] MultiKey(params byte[][] keys)
        {
            string temp = keys[0].AsString();

            for (int i = 1; i < keys.Length; i++)
            {
                temp = string.Concat(temp, "_", keys[i].AsString());
            }

            return temp.AsByteArray();
        }
        #endregion

        #region Private
        private static void OnRefund(byte[] address, BigInteger amount)
        {
            if(Refund != null) Refund(address, amount);
            Runtime.Notify("REFUND".AsByteArray(), address, amount.AsByteArray());
        }
        private static void OnFunded(byte[] icoShareId)
        {
            if (Funded != null) Funded(icoShareId);
            Runtime.Notify("FUNDED".AsByteArray(), icoShareId);
        }
        #endregion

        public static Object Main(string operation, params object[] args)
        {
            if (Runtime.Trigger == TriggerType.Verification)
            {
                if (Owner.Length == 20)
                {
                    // if param Owner is script hash
                    return Runtime.CheckWitness(Owner);
                }
                else if (Owner.Length == 33)
                {
                    // if param Owner is public key
                    byte[] signature = operation.AsByteArray();
                    return VerifySignature(signature, Owner);
                }
            }
            else if (Runtime.Trigger == TriggerType.Application)
            {
                //if (operation == "deploy") return Deploy();
                //if (operation == "mintTokens") return MintTokens();
                //if (operation == "totalSupply") return TotalSupply();
                //if (operation == "name") return Name();
                //if (operation == "symbol") return Symbol();
                //if (operation == "transfer")
                //{
                //    if (args.Length != 3) return false;
                //    byte[] from = (byte[])args[0];
                //    byte[] to = (byte[])args[1];
                //    BigInteger value = (BigInteger)args[2];
                //    return Transfer(from, to, value);
                //}
            }

            //you can choice refund or not refund
            byte[] sender = GetSender();
            ulong contribute_value = GetContributeValue();
            if (contribute_value > 0 && sender.Length != 0)
            {
                OnRefund(sender, contribute_value);
            }
            return false;
        }


        //START NEW ICO
        public static bool StartNewIcoShare(
            byte[] icoShareId, byte[] tokenScriptHash,
            BigInteger startTime, BigInteger endTime,
            BigInteger contributionBundle, BigInteger minContribution, BigInteger maxContribution)
        {
            //Check parameters
            if (icoShareId.Length != IdLenght || startTime < Now() || endTime < startTime) return false;

            //Check if id already used
            var existingId = GetFromStorage(icoShareId);
            if (existingId != null) return false;

            //Set Ico Share Info
            PutOnStorage(icoShareId, POSTFIX_STATUS, ACTIVE);
            PutOnStorage(icoShareId, POSTFIX_TOKENHASH, tokenScriptHash);
            PutOnStorage(icoShareId, POSTFIX_STARTDATE, startTime.AsByteArray());
            PutOnStorage(icoShareId, POSTFIX_ENDDATE, endTime.AsByteArray());
            PutOnStorage(icoShareId, POSTFIX_BUNDLE, contributionBundle.AsByteArray());
            PutOnStorage(icoShareId, POSTFIX_MINCONT, minContribution.AsByteArray());
            PutOnStorage(icoShareId, POSTFIX_MAXCONT, maxContribution.AsByteArray());
            PutOnStorage(icoShareId, POSTFIX_CURRENTCONT, ((BigInteger)0).AsByteArray());

            return true;
        }

        //GET CURRENT CONTRIBUTION
        public static BigInteger GetCurrentContribution(byte[] icoShareId)
        {
            return GetFromStorage(icoShareId, POSTFIX_CURRENTCONT).AsBigInteger();
        }

        //SEND CONTRIBUTION
        public static bool SendContribution(byte[] icoShareId)
        {
            //Sender's address
            byte[] sender = GetSender();

            //Contribute asset is not neo
            if (sender.Length == 0) return false;

            //Get contribution value
            BigInteger contributeValue = GetContributeValue();

            //Check if IcoShare funded
            var isIcoShareFunded = GetFromStorage(icoShareId, POSTFIX_STATUS);
            if (isIcoShareFunded.AsString() != ACTIVE.AsString())
            {
                OnRefund(sender, contributeValue);
                return false;
            }

            //Check enddata
            BigInteger endDate = GetFromStorage(icoShareId, POSTFIX_ENDDATE).AsBigInteger();
            if (endDate < Now())
            {
                OnRefund(sender, contributeValue);
                return false;
            }

            //IcoShare details 
            BigInteger icoShareCurrentAmount = GetFromStorage(icoShareId, POSTFIX_CURRENTCONT).AsBigInteger();
            BigInteger icoShareBundle = GetFromStorage(icoShareId, POSTFIX_BUNDLE).AsBigInteger();
            BigInteger icoShareMax = GetFromStorage(icoShareId, POSTFIX_MAXCONT).AsBigInteger();
            BigInteger sendersCurrentCont = GetFromStorage(MultiKey(icoShareId, sender)).AsBigInteger();

            //Decide to the contribution
            BigInteger contribution = 0;

            //Check maximum contribution for sender
            if (sendersCurrentCont + contributeValue > icoShareMax)
            {
                //User reached to his/her maximum, refund more than icoShareMax
                var calc = icoShareMax - sendersCurrentCont;
                
                BigInteger refundAmount = contributeValue - calc;
                OnRefund(sender, refundAmount);

                contribution = calc;
            }
            else
            {
                contribution = contributeValue;
            }

            //Check if IcoShare current amount reaches full
            if (icoShareCurrentAmount + contribution > icoShareBundle)
            {
                //User reached to icoShare bundle amount, refund more than icoShareBundle

                var calc = icoShareBundle - icoShareCurrentAmount;

                //Refund 
                BigInteger refund = contribution - calc;
                OnRefund(sender, refund);

                contribution = calc;
            }

            //Add/Update user's current contribution
            if (sendersCurrentCont > 0)
            {
                //Update sender's contribution
                PutOnStorage(MultiKey(icoShareId, sender), (sendersCurrentCont + contribution).AsByteArray());
            }
            else
            {
                //Add new contribution amount to sender
                PutOnStorage(MultiKey(icoShareId, sender), contribution.AsByteArray());

                //Add to icoshare's contributors
                PutItemOnStorageList(icoShareId, POSTFIX_CONTRIBUTORS, sender);
            }
            
            //Update IcoShare's current value
            BigInteger icoShareNewAmount = icoShareCurrentAmount + contribution;
            PutOnStorage(icoShareId, POSTFIX_CURRENTCONT, icoShareNewAmount.AsByteArray());

            //Check if IcoShare completed 
            if (icoShareNewAmount == icoShareBundle)
            {
                PutOnStorage(icoShareId, POSTFIX_STATUS, FUNDED);
                OnFunded(icoShareId);
            }
            
            return true;
        }
        
        //REFUND, ICOSHARE IS UNSUCCESFULL
        //Invoked by owner when time limit reached
        public static bool RefundUnsuccesfullIcoShare(byte[] icoShareId)
        {
            if (!IsOwner()) return false;

            //Get contibutor list 
            var key = string.Concat(icoShareId.AsString(), POSTFIX_CONTRIBUTORS);
            var contributors = GetListFromStorage(key.AsByteArray(), SenderAddresLenght);

            //Refund every contribution
            for (int i = 0; i < contributors.Length; i++)
            {
                var amount = GetFromStorage(MultiKey(icoShareId, contributors[i])).AsBigInteger();
                OnRefund(contributors[i], amount);
            }
            
            //Cancel IcoShare
            PutOnStorage(icoShareId, POSTFIX_STATUS, NOTFUNDED);

            return true;
        }
        
        public bool DistributeNep5Tokens(byte[] icoShareId)
        {
            if (!IsOwner()) return false;
            if (GetFromStorage(icoShareId, POSTFIX_STATUS) != FUNDED)
            {
                //TODO : Refun Nep5Token 
                return false;
            }

            //Get icoShare details 

            //Get contributed Nep5Token details 

            //Distribute to contributers' addresses

            return true;
        }

        public void GetToken()
        {
            //Get token script hash, 

            //Get token amount 

            //Find ico share by token script hash

            //Distribute tokens 
        }
    }
}
