using System.Collections.Generic;

namespace Valuator
{
    public interface IRedisStorage
    {
        void Store(string key, string value);
        string Load(string key);
        List<string> GetKeys();
    }
}