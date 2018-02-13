using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartContractEmulator
{
    public class ExecutionEngine
    {
        private static Transaction _scriptContainer;

        public static byte[] ExecutingScriptHash => "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".ToScriptHash();
        public static Transaction ScriptContainer { get => _scriptContainer; }

        private static readonly byte[] NeoAssetId = { 155, 124, 255, 218, 166, 116, 190, 174, 15, 147, 14, 190, 96, 133, 175, 144, 147, 229, 254, 86, 179, 74, 92, 34, 12, 205, 207, 110, 252, 51, 111, 197 };

        
        static ExecutionEngine()
        {
            _scriptContainer = new Transaction(
                new TransactionOutput[] { new TransactionOutput() { } },
                new TransactionOutput[] { new TransactionOutput() { } });
        }

        public static ulong _contributeValue;
        /// <summary>
        /// For unit test
        /// </summary>
        public static ulong ConributedNeoValue { get { return _contributeValue; } set {
                _contributeValue = value;
                _scriptContainer._transactionOutput[0].AssetId = NeoAssetId;
                _scriptContainer._transactionOutput[0].Value = value;
                _scriptContainer._transactionOutput[0].ScriptHash = ExecutionEngine.ExecutingScriptHash;

            } }
        
        private static byte[] _sender;
        /// <summary>
        /// For unit test
        /// </summary>
        public static byte[] Sender { get { return _sender; } set {
                _sender = value;
                _scriptContainer._references[0].AssetId = NeoAssetId;
                _scriptContainer._references[0].ScriptHash = value;
            }
        }
        
        /// <summary>
        /// For unit test 
        /// </summary>
        /// <returns></returns>
        public static ulong GetContibuteValue()
        {
            return ConributedNeoValue;
        }
    }

}
