namespace Bulwark.Integration
{
    public class MessageQueueOptions
    {
        public MessageQueueOptions()
        {
            Type = Types.MessageQueueType.InMemory;
            RabbitMqPort = 5672;
            DiskQueueLocation = "disk_queue";
            LiteDBLocation = "lite.db";
            SqlLiteDBLocation = "sqlite.db";
        }
        
        public Types.MessageQueueType Type { get; set; }
        
        public string RabbitMqHost { get; set; }
        
        public string RabbitMqUsername { get; set; }
        
        public string RabbitMqPassword { get; set; }
        
        public int RabbitMqPort { get; set; }
        
        public string DiskQueueLocation { get; set; }
        
        public string LiteDBLocation { get; set; }
        
        public string SqlLiteDBLocation { get; set; }
        
        public class Types
        {
            public enum MessageQueueType
            {
                InMemory, // Not recommended for production
                RabbitMQ,
                DiskQueue,
                LiteDB,
                Sqlite
            }
        }
    }
}