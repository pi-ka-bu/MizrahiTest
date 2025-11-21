using System.Threading.Tasks;
using TestAPI.Models;

namespace TestAPI.Interfaces
{
    public interface ICalculationService
    {
        Task<MathResponse> AddAsync(MathRequest request);

        Task<MathResponse> SubtractAsync(MathRequest request);

        Task<MathResponse> MultiplyAsync(MathRequest request);

        Task<MathResponse> DivideAsync(MathRequest request);
    }
}
