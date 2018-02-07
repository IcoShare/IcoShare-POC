using System;
using System.Linq;

namespace SmartContractEmulator
{
    public class Runtime
    {
        public static int Now { get; set; }
        public static TriggerType Trigger { get; set; }

        public static bool CheckWitness(byte[] address) => address.AsString() == ExecutionEngine.Sender.AsString();

        public delegate void NotifiedEventHandler(byte[][] messages);
        public static event NotifiedEventHandler Notified;

        public static void Notify(params byte[][] messages)
        {
            messages.ToList().ForEach( x=> Console.Write( string.Concat(x.AsString(), "|")));
            Console.WriteLine();

            Notified?.Invoke(messages);
        }
    }

}
