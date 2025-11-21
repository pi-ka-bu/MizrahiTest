using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestAPI.Interfaces;
using TestAPI.Models;

namespace TestAPI.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ILogger<CalculationService> _logger;

        public CalculationService(ILogger<CalculationService> logger)
        {
            _logger = logger;
        }

        public Task<MathResponse> AddAsync(MathRequest request)
        {
            _logger.LogDebug("Performing addition: {X} + {Y}", request.X, request.Y);
            var result = request.X + request.Y;
            return Task.FromResult(CreateResponse(request, result, "add"));
        }

        public Task<MathResponse> SubtractAsync(MathRequest request)
        {
            _logger.LogDebug("Performing subtraction: {X} - {Y}", request.X, request.Y);
            var result = request.X - request.Y;
            return Task.FromResult(CreateResponse(request, result, "subtract"));
        }

        public Task<MathResponse> MultiplyAsync(MathRequest request)
        {
            _logger.LogDebug("Performing multiplication: {X} * {Y}", request.X, request.Y);
            var result = request.X * request.Y;
            return Task.FromResult(CreateResponse(request, result, "multiply"));
        }

        public Task<MathResponse> DivideAsync(MathRequest request)
        {
            if (request.Y == 0)
            {
                _logger.LogWarning("Attempted division by zero: {X} / {Y}", request.X, request.Y);
                throw new DivideByZeroException("Cannot divide by zero");
            }

            _logger.LogDebug("Performing division: {X} / {Y}", request.X, request.Y);
            var result = request.X / request.Y;
            return Task.FromResult(CreateResponse(request, result, "divide"));
        }

        private MathResponse CreateResponse(MathRequest request, decimal result, string operation)
        {
            return new MathResponse
            {
                RequestId = Guid.NewGuid().ToString(),
                Operation = operation,
                X = request.X,
                Y = request.Y,
                Result = result,
                FromCache = false,
                Timestamp = DateTime.UtcNow,
            };
        }
    }
}
