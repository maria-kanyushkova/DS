using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

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

        public IActionResult OnPost(string text)
        {
            if (string.IsNullOrEmpty(text)) Redirect("/");

            var id = Guid.NewGuid().ToString();

            var similarityKey = "SIMILARITY-" + id;
            var similarity = GetSimilarity(text, id);
            _redisStorage.Store(similarityKey, similarity.ToString());

            var textKey = "TEXT-" + id;
            _redisStorage.Store(textKey, text);

            var rankKey = "RANK-" + id;
            _redisStorage.Store(rankKey, GetRank(text).ToString());
            return Redirect($"summary?id={id}");
        }
        
        private int GetSimilarity(string text, string id)
        {
            var keys = _redisStorage.GetKeys();

            return keys.Any(item => 
                item.Substring(0, 5) == "TEXT-" && _redisStorage.Load(item) == text) ? 1 : 0;
        }

        private static double GetRank(string text)
        {
            var notLetterCount = text.Count(ch => !char.IsLetter(ch));

            return (double) notLetterCount / text.Length;
        }
    }
}
