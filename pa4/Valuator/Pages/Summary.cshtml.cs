using System;
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
            _logger.LogDebug(id);

            var rankKey = Const.RankTitleKey + id;

            Similarity = Convert.ToDouble(_redisStorage.Load(Const.SimilarityTitleKey + id));

            if (_redisStorage.IsKeyExist(rankKey))
                Rank = Convert.ToDouble(_redisStorage.Load(rankKey));
            else
                _logger.LogWarning("RankKey {rankKey} doesn't exists", rankKey);
        }
    }
}