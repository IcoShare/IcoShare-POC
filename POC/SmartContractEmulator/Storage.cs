using System.Collections.Generic;

namespace SmartContractEmulator
{
    public class Storage
    {
        public class StorageContext { }
        public static StorageContext CurrentContext { get; }

        public static Dictionary<string, string> MemoryStorage = new Dictionary<string, string>() { };

        public static void Put(StorageContext context, byte[] key, byte[] value)
        {
            var keyStr = key.AsString();
            var valueStr = value.AsString();

            if (MemoryStorage.ContainsKey(keyStr))
            {
                MemoryStorage[keyStr] = value.AsString();
            }
            else
            {
                MemoryStorage.Add(key.AsString(), valueStr);
            }
        }

        public static byte[] Get(StorageContext context, byte[] key)
        {
            if (!MemoryStorage.ContainsKey(key.AsString())) return null;
            return MemoryStorage[key.AsString()].AsByteArray();
        }

    }

}
