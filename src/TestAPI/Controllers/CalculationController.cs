using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestAPI.Interfaces;
using TestAPI.Models;
using static TestAPI.Models.MathRequest;

namespace TestAPI.Controllers
{
    [ApiController]
    [Route("api/math")]
    [Authorize]
    public class CalculationController : ControllerBase
    {
        private readonly ICalculationService _calculationService;
        private readonly ILogger<CalculationController> _logger;

        public CalculationController(
            ICalculationService calculationService,
            ILogger<CalculationController> logger
        )
        {
            _calculationService = calculationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(
            [FromBody] MathRequest request,
            [FromHeader(Name = "X-ArithmeticOp-ID")] string arithmeticOpId
        )
        {
            if (string.IsNullOrWhiteSpace(arithmeticOpId))
            {
                _logger.LogWarning("Request missing header!");
                return BadRequest(new { Error = "Authentication header is required." });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for request");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation(
                    "Processing {Operation} operation with X-ArithmeticOp-ID: {OpId}",
                    request.Operation,
                    arithmeticOpId
                );

                MathResponse response = request.Operation switch
                {
                    OperationEnum.AddEnum => await _calculationService.AddAsync(request),
                    OperationEnum.SubtractEnum => await _calculationService.SubtractAsync(request),
                    OperationEnum.MultiplyEnum => await _calculationService.MultiplyAsync(request),
                    OperationEnum.DivideEnum => await _calculationService.DivideAsync(request),
                    _ => throw new InvalidOperationException("Unknown operation"),
                };

                return Ok(response);
            }
            catch (DivideByZeroException ex)
            {
                _logger.LogWarning(ex, "Division by zero attempted");
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing calculation");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
