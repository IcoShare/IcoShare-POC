using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartContractEmulator
{
    public class Transaction
    {
        public TransactionOutput[] _references;
        public TransactionOutput[] _transactionOutput;

        public Transaction(TransactionOutput[] transactionOutput, TransactionOutput[] references)
        {
            _transactionOutput = transactionOutput;
            _references = references;
        }

        public TransactionOutput[] GetReferences() => _references;
        public TransactionOutput[] GetOutputs() => _transactionOutput;
    }
}
