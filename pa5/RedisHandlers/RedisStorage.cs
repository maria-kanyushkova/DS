using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using StackExchange.Redis;

namespace RedisHandlers
{
    public class RedisStorage : IRedisStorage
    {
        private readonly IConnectionMultiplexer _connectionMain;
        private readonly Dictionary<string, IConnectionMultiplexer> _connections;

        public RedisStorage()
        {
            _connectionMain = ConnectionMultiplexer.Connect(Configs.HostName);
            _connections = new Dictionary<string, IConnectionMultiplexer>
            {
                {
                    Configs.SegmentRus,
                    ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(Configs.SegmentRus))
                },
                {
                    Configs.SegmentEu,
                    ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(Configs.SegmentEu))
                },
                {
                    Configs.SegmentOther,
                    ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(Configs.SegmentOther))
                }
            };
        }

        public void Store(string key, string value, string sharedKey)
        {
            var db = _connections[sharedKey].GetDatabase();
            if (key.StartsWith(Const.TextTitleKey)) db.SetAdd(Const.TextTitleKey, value);

            db.StringSet(key, value);
        }

        public void StoreShard(string key, string sharedKey)
        {
            _connectionMain.GetDatabase().StringSet(key, sharedKey);
        }

        public string Load(string key, string sharedKey)
        {
            return _connections[sharedKey].GetDatabase().StringGet(key);
        }

        public string LoadShard(string key)
        {
            return _connectionMain.GetDatabase().StringGet(key);
        }

        public bool HasValueDuplicates(string value)
        {
            return _connections.Any(item => item.Value.GetDatabase().SetContains(Const.TextTitleKey, value));
        }

        public bool IsKeyExist(string key, string sharedKey)
        {
            return _connections[sharedKey].GetDatabase().KeyExists(key);
        }
    }
}