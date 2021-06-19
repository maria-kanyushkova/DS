using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using RedisHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Valuator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IRedisStorage _redisStorage;

        public IndexModel(ILogger<IndexModel> logger, IRedisStorage storage)
        {
            _logger = logger;
            _redisStorage = storage;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string text)
        {
            if (string.IsNullOrEmpty(text)) Redirect("/");

            var id = Guid.NewGuid().ToString();
            var similarity = GetSimilarity(text, id);

            _redisStorage.Store(Const.SimilarityTitleKey + id, similarity.ToString());
            _redisStorage.Store(Const.TextTitleKey + id, text);

            await CreateEventForSimilarity(id, similarity);
            await CreateRankCalculator(id);

            return Redirect($"summary?id={id}");
        }

        private int GetSimilarity(string text, string id)
        {
            var keys = _redisStorage.GetKeys();

            return keys.Any(item =>
                item.Substring(0, 5) == Const.TextTitleKey && _redisStorage.Load(item) == text)
                ? 1
                : 0;
        }

        private async Task CreateRankCalculator(string id)
        {
            var tokenSource = new CancellationTokenSource();
            var connectionFactory = new ConnectionFactory();
            using (var connection = connectionFactory.CreateConnection())
            {
                if (!tokenSource.IsCancellationRequested)
                {
                    var data = Encoding.UTF8.GetBytes(id);
                    connection.Publish(Const.RankProcess, data);
                }

                connection.Drain();
                connection.Close();
            }
        }

        private async Task CreateEventForSimilarity(string id, int similarity)
        {
            string message = $"Event: SimilarityCalculated, context id: {id}, similarity: {similarity}";
            ConnectionFactory connectionFactory = new ConnectionFactory();
            using (var connection = connectionFactory.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                connection.Publish(Const.BrokerSimilarity, data);

                connection.Drain();
                connection.Close();
            }
        }
    }
}