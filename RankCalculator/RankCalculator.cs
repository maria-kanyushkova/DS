using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Linq;
using System.Text;
using Common;
using Microsoft.Extensions.Logging;
using NATS.Client;
using RedisHandlers;

namespace RankCalculator
{
    public class RankCalculator
    {
        private readonly ILogger<RankCalculator> _logger;
        private readonly IConnection _connection;
        private readonly IRedisStorage _redisStorage;
        private IAsyncSubscription _subscription;

        public RankCalculator(ILogger<RankCalculator> logger, IRedisStorage storage)
        {
            _logger = logger;
            _connection = new ConnectionFactory().CreateConnection();
            _redisStorage = storage;
        }

        public void Run()
        {
            _subscription = _connection.SubscribeAsync(Const.RankProcess, "rank_calculator", (sender, args) =>
            {
                var id = Encoding.UTF8.GetString(args.Message.Data);
                var shard = _redisStorage.LoadShard(id);
                var textKey = Const.TextTitleKey + id;

                if (!_redisStorage.IsKeyExist(textKey, shard))
                {
                    _logger.LogWarning("Text key {textKey} doesn't exists", textKey);
                    return;
                }

                var text = _redisStorage.Load(textKey, shard);
                var rank = CalculateRank(text).ToString();

                _logger.LogInformation("Shard: " + shard + ", id: " + id + ", text: " + text + ", rank: " + rank);

                _redisStorage.Store(Const.RankTitleKey + id, rank, shard);

                var message = $"Event: RankCalculated, context id: {id}, rank: {rank}";
                _connection.Publish(Const.BrokerRank, Encoding.UTF8.GetBytes(message));
            });

            _subscription.Start();

            Console.WriteLine("Press Enter to exit (RankCalculator)");
            Console.ReadLine();

            _subscription.Unsubscribe();

            _connection.Drain();
            _connection.Close();
        }

        private static double CalculateRank(string text)
        {
            var notLettersCount = text.Count(ch => !char.IsLetter(ch));
            return (double) notLettersCount / text.Length;
        }
    }
}