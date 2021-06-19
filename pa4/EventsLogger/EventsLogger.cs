using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using Common;
using System.Text;

namespace EventsLogger
{
    public class EventsLogger
    {
        private ILogger _logger;
        private readonly IConnection _connection;
        private IAsyncSubscription _subscription;

        public EventsLogger(ILogger<EventsLogger> logger)
        {
            _logger = logger;
            _connection = new ConnectionFactory().CreateConnection();
        }

        public void Run()
        {
            _subscription = _connection.SubscribeAsync(Const.BrokerRank, (sender, args) =>
            {
                string message = Encoding.UTF8.GetString(args.Message.Data);
                _logger.LogDebug(message);
            });

            _subscription = _connection.SubscribeAsync(Const.BrokerSimilarity, (sender, args) =>
            {
                string message = Encoding.UTF8.GetString(args.Message.Data);
                _logger.LogDebug(message);
            });

            _subscription.Start();

            Console.WriteLine("Press Enter to exit (EventsLogger)");
            Console.ReadLine();

            _subscription.Unsubscribe();

            _connection.Drain();
            _connection.Close();
        }
    }
}