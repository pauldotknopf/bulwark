namespace Bulwark.Integration
{
    public class MessageQueueOptions
    {
        public MessageQueueOptions()
        {
            Type = Types.MessageQueueType.InMemory;
        }
        
        public Types.MessageQueueType Type { get; set; }
        
        public string RabbitMqServerUrl { get; set; }
        
        public class Types
        {
            public enum MessageQueueType
            {
                InMemory, // Not recommended for production
                RabbitMQ
            }
        }
    }
}