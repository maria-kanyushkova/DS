using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace RedisHandlers
{
    public class RedisStorage : IRedisStorage
    {
        private readonly IConnectionMultiplexer _connection;
        // private readonly IDatabase _db;

        public RedisStorage()
        {
            _connection = ConnectionMultiplexer.Connect(Configs.REDIS_HOSTNAME);
            // _db = _connection.GetDatabase();
        }

        public void Store(string key, string value)
        {
            var db = _connection.GetDatabase();
            
            db.StringSet(key, value);
        }

        public string Load(string key)
        {
            var db = _connection.GetDatabase();
            
            return db.StringGet(key);
        }

        public List<string> GetKeys()
        {
            var keys = _connection.GetServer(Configs.REDIS_HOSTNAME, Configs.REDIS_PORT).Keys();

            return keys.Select(item => item.ToString()).ToList();
        }
        
        public bool IsKeyExist(string key)
        {
            var db = _connection.GetDatabase();
            return db.KeyExists(key);
        }
    }
}