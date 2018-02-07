using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartContractEmulator
{

    public class ExecutionEngine
    {
        public static ulong ConributeValue;
        public static byte[] Sender;

        public static ulong GetContibuteValue()
        {
            return ConributeValue;
        }

        public static byte[] GetSender()
        {
            return Sender;
        }
    }

}
