using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Linq;
using System.Text;
using Common;

namespace RankCalculator
{
    public class RankCalculator
    {
        private readonly ILogger<RankCalculator> _logger;
        private readonly IConnection _connection;
        private readonly IStorage _storage;

        public RankCalculator(ILogger<RankCalculator> logger, IStorage storage)
        {
            _logger = logger;
            _connection = new ConnectionFactory().CreateConnection();
            _storage = storage;
        }

        public void Run()
        {
            var subscription = _connection.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);
                string textKey = "TEXT-" + id;

                if (!_storage.IsKeyExist(textKey))
                {
                    _logger.LogWarning("Text key {textKey} doesn't exists", textKey);
                    return;
                }

                string text = _storage.Load(textKey);
                string rankKey = "RANK-" + id;
                string rank = CalculateRank(text).ToString();

                _logger.LogDebug("Rank {rank} with key {rankKey} by text id {id}", rank, rankKey, id);

                _storage.Store(rankKey, rank);
            });

            subscription.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            subscription.Unsubscribe();

            _connection.Drain();
            _connection.Close();
        }

        private static double CalculateRank(string text)
        {
            var notLettersCount = text.Count(ch => !char.IsLetter(ch));
            return (double)notLettersCount / text.Length;
        }
    }
}