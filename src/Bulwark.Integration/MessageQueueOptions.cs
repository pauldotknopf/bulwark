namespace Bulwark.Integration
{
    public class MessageQueueOptions
    {
        public MessageQueueOptions()
        {
            MessageQueueType = Types.MessageQueueType.InMemory;
        }
        
        public Types.MessageQueueType MessageQueueType { get; set; }
        
        public string RabbitMqServerUrl { get; set; }
        
        public class Types
        {
            public enum MessageQueueType
            {
                InMemory, // Not recommended for production
                RabbitMq
            }
        }
    }
}