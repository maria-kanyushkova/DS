using Microsoft.Extensions.Logging;
using RedisHandlers;

namespace RankCalculator
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var loggerFactory =
                LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)))
            {
                var _redisStorage = new RedisStorage();
                var rankCalculator = new RankCalculator(loggerFactory.CreateLogger<RankCalculator>(), _redisStorage);
                rankCalculator.Run();
            }
        }
    }
}