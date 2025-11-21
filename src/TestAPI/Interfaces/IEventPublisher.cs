using System.Threading.Tasks;

namespace TestAPI.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishCalculationEventAsync(string operation, decimal x, decimal y, decimal result);
    }
}
