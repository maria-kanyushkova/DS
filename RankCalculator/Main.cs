using Common;
using Microsoft.Extensions.Logging;

namespace RankCalculator
{
    public class Main
    {
        static void Main(string[] args)
        {
            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)))
            {
                var storage = new Storage();
                var rankCalculator = new RankCalculator(loggerFactory.CreateLogger<RankCalculator>(), storage);
                rankCalculator.Run();
            }
        }
    }
}