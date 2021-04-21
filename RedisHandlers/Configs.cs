using System;

namespace RedisHandlers
{
    public static class Configs
    {
        public const string SegmentRus = "DB_RUS";
        public const string SegmentEu = "DB_EU";
        public const string SegmentOther = "DB_OTHER";
        
        public static string HostName
        {
            get
            {
                var hostName = Environment.GetEnvironmentVariable("MACHINE_IP");
                return string.IsNullOrWhiteSpace(hostName) ? "localhost" : hostName;
            }
        }
    }
}