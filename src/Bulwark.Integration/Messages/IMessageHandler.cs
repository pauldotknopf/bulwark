using System.Threading.Tasks;

namespace Bulwark.Integration.Messages
{
    public interface IMessageHandler<in T> where T : class
    {
        Task Handle(T message);
    }
}