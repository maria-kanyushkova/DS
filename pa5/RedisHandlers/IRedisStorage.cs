using System.Collections.Generic;

namespace RedisHandlers
{
    public interface IRedisStorage
    {
        void Store(string key, string value, string sharedKey);
        void StoreShard(string key, string sharedKey);
        string Load(string key, string sharedKey);
        string LoadShard(string key);
        bool HasValueDuplicates(string value);
        bool IsKeyExist(string textKey, string sharedKey);
    }
}