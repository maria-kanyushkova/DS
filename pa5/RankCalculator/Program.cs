using Microsoft.Extensions.Logging;
using RedisHandlers;

namespace RankCalculator
{
    public class Program
    {
        private static void Main(string[] args)
        {
            using (var loggerFactory =
                LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)))
            {
                var redisStorage = new RedisStorage();
                var rankCalculator = new RankCalculator(loggerFactory.CreateLogger<RankCalculator>(), redisStorage);
                rankCalculator.Run();
            }
        }
    }
}