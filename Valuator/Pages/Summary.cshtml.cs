using System;
using System.Threading;
using Common;
using RedisHandlers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Valuator.Pages
{
    public class SummaryModel : PageModel
    {
        private readonly ILogger<SummaryModel> _logger;
        private readonly IRedisStorage _redisStorage;


        public SummaryModel(ILogger<SummaryModel> logger, IRedisStorage redisStorage)
        {
            _logger = logger;
            _redisStorage = redisStorage;
        }

        public double Rank { get; set; }
        public double Similarity { get; set; }

        public void OnGet(string id)
        {
            var shard = _redisStorage.LoadShard(id);
            _logger.LogDebug($"{shard} : {id} - SummaryGet");

            var rankKey = Const.RankTitleKey + id;

            Similarity = Convert.ToDouble(_redisStorage.Load(Const.SimilarityTitleKey + id, shard));
            
            for (int retryCount = 0; retryCount < 100; retryCount++)
            {
                Thread.Sleep(10);
                if (_redisStorage.IsKeyExist(rankKey, shard))
                {
                    Rank = Convert.ToDouble(_redisStorage.Load(rankKey, shard));
                    return;
                }
            }
            _logger.LogWarning($"RankKey {rankKey} doesn't exists in [{shard}]");
        }
    }
}