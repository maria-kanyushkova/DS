using System.Collections.Generic;

namespace RedisHandlers
{
    public interface IRedisStorage
    {
        void Store(string key, string value);
        string Load(string key);
        List<string> GetKeys();
        bool IsKeyExist(string textKey);
    }
}