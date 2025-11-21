using System.Threading.Tasks;

namespace TestAPI.Interfaces
{
    public interface IMetadataService
    {
        Task<string> GetOperationMetadataAsync(string operation);
    }
}
