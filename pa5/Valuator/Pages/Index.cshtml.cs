﻿using System;
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

        public async Task<IActionResult> OnPost(string text, string segment)
        {
            _logger.LogDebug(text);
            if (string.IsNullOrEmpty(text)) Redirect("/");

            var id = Guid.NewGuid().ToString();
            _logger.LogInformation($"{segment} : {id} - OnPost");
            var similarity = GetSimilarity(text, id);

            _redisStorage.StoreShard(id, segment);
            _redisStorage.Store(Const.SimilarityTitleKey + id, similarity.ToString(), segment);
            _redisStorage.Store(Const.TextTitleKey + id, text, segment);

            await CreateEventForSimilarity(id, similarity);
            await CreateRankCalculator(id);

            return Redirect($"summary?id={id}");
        }

        private int GetSimilarity(string value, string id)
        {
            return _redisStorage.HasValueDuplicates(value) ? 1 : 0;
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