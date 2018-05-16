using System.Threading.Tasks;

namespace Bulwark.Integration.Messages
{
    public interface IMessageSender
    {
        Task Send<T>(T message) where T : class;
    }
}